using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HiQPdf;
using LFE.Application.Services.Base;
using LFE.Application.Services.ExternalProviders;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;

namespace LFE.Application.Services
{
	public class CertificateServices : ServiceBase, ICertificateAdminServices, IStudentSertificateCervices
	{
		private const string KEY_SEPARATOR = "$";
		private readonly IEmailServices _emailServices;
		private readonly IAmazonEmailWrapper _amazonEmailWrapper;
	    private readonly IS3Wrapper _s3Wrapper;
		private readonly EncryptionServices _encryptionServices = new EncryptionServices();

		public CertificateServices()
		{
			_emailServices      = DependencyResolver.Current.GetService<IEmailServices>();
			_amazonEmailWrapper = DependencyResolver.Current.GetService<IAmazonEmailWrapper>();
		    _s3Wrapper          = DependencyResolver.Current.GetService<IS3Wrapper>();
		}

		private static List<CertificateTemplateDTO> _templates = new List<CertificateTemplateDTO>();
		private static void initTemplates()
		{
			_templates = new List<CertificateTemplateDTO>();

			using (var context = new lfeAuthorEntities())
			{
				var templates = context.CERT_TemplatesLOV.Where(x => x.IsActive).ToList();

				foreach (var entity in templates)
				{
					_templates.Add(entity.Entity2TemplateDto());
				}
			}
		}

		#region ICertificateAdminServices
		public List<CertificateTemplateDTO> TempatesLOV
		{
			get
			{
				if (_templates.Any()) return _templates;
				
				initTemplates();

				return _templates;
			}
		}

		public CertificateDTO GetCourseCertificate(int courseId)
		{
			try
			{
				
				var entity = CertificateRepository.GetMany(x=>x.CourseId == courseId).FirstOrDefault();

				if (entity == null)
				{
					var courseEntity = CourseRepository.GetById(courseId);

					return courseEntity != null ? new CertificateDTO(courseId,courseEntity.CourseName,this.GetCurrentUserName()) : new CertificateDTO{IsValid = false,Message = "Course not found"};
				}

				var token = entity.Entity2CertificateDto(StudentCertificatesRepository.IsAny(x=>x.CertificateId == entity.CertificateId),CourseQuizzesRepository.IsAny(x=>x.CourseId == entity.CourseId && x.AttachCertificate));

				return token;
			}
			catch (Exception ex)
			{
				Logger.Error("get certificate token", courseId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				return new CertificateDTO
				{
					IsValid = false
					,Message = FormatError(ex)
				};
			}
		}

		public bool SaveCertificate(CertificateDTO token, out string error)
		{            
			try
			{
				var entity = CertificateRepository.GetMany(x => x.CourseId == token.CourseId).FirstOrDefault();

				if (entity == null)
				{
					entity = token.Token2CertificateEntity();

					CertificateRepository.Add(entity);

					if (!CertificateRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

					token.CertificateId = entity.CertificateId;

					return true;
				}

				entity.UpdateCertificateEntity(token);

				return CertificateRepository.UnitOfWork.CommitAndRefreshChanges(out error) && _deattchCertificateFromQuizes(token.IsActive, token.CourseId, out  error);
			}
			catch (Exception ex)
			{
				Logger.Error("save certificate ", token.CourseId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				error = FormatError(ex);
				return false;
			}
		}

		private bool _deattchCertificateFromQuizes(bool isActive,int courseId, out string error)
		{
			error = string.Empty;

			if (isActive) return true;
			try
			{
				var quizzes = CourseQuizzesRepository.GetMany(x=>x.CourseId ==courseId && x.AttachCertificate).ToList();

				foreach (var entity in quizzes)
				{
					entity.UpdateQuizCertAttachState(false);
				}

				return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				return false;
			}
		}

		public bool DeleteCertificate(int certId, out string error)
		{
			try
			{
				if (StudentCertificatesRepository.IsAny(x => x.CertificateId == certId))
				{
					error = "Certificate already sent to students and can't be deleted. You could disable it.";
					return false;
				}

				CertificateRepository.Delete(x=>x.CertificateId == certId);
				return CertificateRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception ex)
			{
				Logger.Error("delete certificate", certId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				error = FormatError(ex);
				return false;
			}
		}

		#region reports

		public List<StudentCertificateDTO> FindStudentCertificatesByAuthor(int authorUserId)
		{
			try
			{
				return StudentCertificatesViewRepository.GetMany(x => x.AuthorUserId == authorUserId).Select(x=>x.Entity2StudentCertificateDto()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("FindStudentCertificatesByAuthor " + authorUserId, CurrentUserId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				return new List<StudentCertificateDTO>();
			}
		}
		#endregion
		#endregion

		#region IStudentSertificateCervices
		public StudentCertificateDTO GetStudentCertificate(Guid attemptId, int userId)
		{
			try
			{
				var attemptEntity = StudentQuizAttemptsRepository.GetById(attemptId);

				if (attemptEntity == null)
				{
					Logger.Warn("GetStudentCertificate for attempt " + attemptId + ":: attempt entity not found", CommonEnums.LoggerObjectTypes.Quiz);
					return new StudentCertificateDTO { IsValid = false, Message = "attempt entity not found" };
				}

				var courseId = attemptEntity.QZ_StudentQuizzes.QZ_CourseQuizzes.CourseId;

				return GetStudentCertificate(courseId,userId);
			}
			catch (Exception ex)
			{
				Logger.Error("GetStudentCertificate for attempt " + attemptId, userId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				return new StudentCertificateDTO { IsValid = false, Message = FormatError(ex) };
			}
		}

		public StudentCertificateDTO GetStudentCertificate(int courseId, int userId)
		{
			try
			{
				var studentCertificate = StudentCertificatesViewRepository.Get(x => x.CourseId == courseId && x.StudentUserId == userId);

				if (studentCertificate != null) return studentCertificate.Entity2StudentCertificateDto();

				var certificateEntity = CertificateRepository.GetMany(x => x.CourseId == courseId).First();

				if (certificateEntity == null)
				{
					Logger.Warn("GetStudentCertificate for course " + courseId + ":: certificate entity not found", CommonEnums.LoggerObjectTypes.Quiz);
					return new StudentCertificateDTO { IsValid = false, Message = "certificate entity not found" };
				}

				var studentCertificateEntity = certificateEntity.CertificateEntity2StudentCertificateEntity(userId);

				StudentCertificatesRepository.Add(studentCertificateEntity);

				string error;

				return StudentCertificatesRepository.UnitOfWork.CommitAndRefreshChanges(out error) ? 
													StudentCertificatesViewRepository.Get(x => x.CourseId == courseId && x.StudentUserId == userId).Entity2StudentCertificateDto() :
													new StudentCertificateDTO { IsValid = false, Message = error };
			}
			catch (Exception ex)
			{
				Logger.Error("GetStudentCertificate for course " + courseId, userId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				return new StudentCertificateDTO { IsValid = false, Message = FormatError(ex) };
			}
		}

		public StudentCertificateDTO GetStudentCertificate(Guid studentCertId)
		{
			try
			{
				return StudentCertificatesViewRepository.Get(x => x.StudentCertificateId == studentCertId).Entity2StudentCertificateDto();
			}
			catch (Exception ex)
			{
				Logger.Error("GetStudentCertificate by id " + studentCertId, studentCertId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				return new StudentCertificateDTO { IsValid = false, Message = FormatError(ex) };
			}
		}

		public bool SendStudentCertificate(StudentCertificateDTO token, string certificateBody, out string error)
		{
		    try
		    {
		        var strKey = string.Format("{0}{3}{1}{3}{2}", token.StudentCertificateId, token.StudentInfo.UserId,token.CertificateId, KEY_SEPARATOR);
		        var key    = _encryptionServices.EncryptText(strKey);
		        token.Key  = Uri.EscapeDataString(key);

		        token.OnlineCertificateUrl = $"{"https:"}//{new Uri(Utils.GetKeyValue("baseUrl")).Authority}/Widget/{"User"}/{"Certificate"}?key={Uri.EscapeDataString(key)}";


		        long emailId;
		        _emailServices.SaveStudentCertificateMessage(token, certificateBody, out emailId, out error);

		        if (emailId < 0) return false;

		        var pdfConverter = new StandardPdfRenderer();

		       // var fileName = $"Certificate_{token.StudentInfo.UserId}_{ShortGuid.NewGuid()}.pdf";
                //				var virtualUrl = "~/Certificates/" + fileName;
                //				var path       = HttpContext.Current.Server.MapPath(virtualUrl);
                //
                //				//var bytes = pdfConverter.Html2Pdf(certificateBody);
                //				//File.WriteAllBytes(path, bytes);
                //
                //                if (File.Exists(path)) TryDeleteFile(path);
                //
                //                var pdfDoc = pdfConverter.Html2PdfDoc(certificateBody, PdfPageSize.A4);
                //
                //				pdfDoc.WriteToFile(path);
                //
                //                pdfDoc.Close();
                //
                //                pdfDoc.ReleaseSourceDoc();

                //                var sended = _amazonEmailWrapper.SendEmailWithAttachment(emailId, path, out error);

                var outstream = pdfConverter.Html2PdfStream(certificateBody);
//                
//                var filePath = $"{FileEnums.eFileOwners.Student}/Certificates/{token.StudentInfo.UserId}/{fileName}";
//                
//                _s3Wrapper.Upload(filePath, "application/octet-stream", outstream, out error);
//                
//                // var fullCertificatePath = $"{S3_ROOT_URL}{S3_BUCKET_NAME}/{filePath}";
//                
//                outstream = pdfConverter.Html2PdfStream(certificateBody);
                
                var sended = _amazonEmailWrapper.SendEmailWithAttachment(emailId, outstream, out error);

                if (sended)
				{
					var entity = StudentCertificatesRepository.GetById(token.StudentCertificateId);

					entity.UpdateCertificateSendDate();

					StudentCertificatesRepository.UnitOfWork.CommitAndRefreshChanges();
				}
                outstream.Close();
                outstream.Dispose();

				return sended;
			}
			catch (Exception ex)
			{
				Logger.Error("Send Student Certificate for course " + token.CourseId, CurrentUserId, ex, CommonEnums.LoggerObjectTypes.Certificate);
				error = FormatError(ex);
				return false;
			}
		}

	    private void TryDeleteFile(string path)
	    {
            try
            {
                File.Delete(path);
            }
            catch (Exception) { }
	    }

		public StudentCertificateDTO GetStudentCertificate(string key)
		{
			try
			{
				var decrypted = _encryptionServices.DecryptText(key);

				var keys = decrypted.Split(Convert.ToChar(KEY_SEPARATOR));

				if (keys.Count() != 3)
				{
					Logger.Warn("GetStudentCertificate by key " + key+ ":: keys missing with count " + keys.Count(), CommonEnums.LoggerObjectTypes.Certificate);
					return new StudentCertificateDTO { IsValid = false, Message = "Certificate not found" };
				}

				Guid sid;
				int userId;
				int certid;

				if (!Guid.TryParse(keys[0], out sid) || !int.TryParse(keys[1], out userId) || !int.TryParse(keys[2], out certid))
				{
					Logger.Warn("GetStudentCertificate by key " + key + ":: key not parsed", CommonEnums.LoggerObjectTypes.Certificate);
					return new StudentCertificateDTO { IsValid = false, Message = "Certificate not found" };
				}

				var certificate = StudentCertificatesViewRepository.Get(x => x.StudentCertificateId == sid && x.StudentUserId == userId && x.CertificateId == certid);

				if (certificate == null)
				{
					Logger.Warn("GetStudentCertificate by key " + key + ":: student certificate not found", CommonEnums.LoggerObjectTypes.Certificate);
					return new StudentCertificateDTO { IsValid = false, Message = "Certificate not found" };
				}

				return certificate.Entity2StudentCertificateDto();
			}
			catch (Exception ex)
			{
				Logger.Error("GetStudentCertificate by key " + key, null, ex, CommonEnums.LoggerObjectTypes.Certificate);
				return new StudentCertificateDTO { IsValid = false, Message = "Certificate not found" };
			}
		}

		public bool IsCourseHasCertificateOnCompletion(int courseId)
		{
			return _HasCertificateOnComplete(courseId);
		}

		#endregion
	}
}
