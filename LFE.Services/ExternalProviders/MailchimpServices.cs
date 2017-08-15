using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;
using MailChimp;
using MailChimp.Helper;
using MailChimp.Lists;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Application.Services.ExternalProviders
{
    public class MailchimpServices : ServiceBase, IMailchimpServices
    {
        private const int MAX_LIST_SUBSCRIBE    = 5000;     // maximum 5k-10k records http://apidocs.mailchimp.com/api/2.0/lists/batch-subscribe.php
        private const int MAX_SEGMENT_SUBSCRIBE = 10000;    // maximum 10k records http://apidocs.mailchimp.com/api/2.0/lists/static-segment-members-add.php


        #region private helpers
        private void HandleAddStaticSegmentMembersError(int userId, StaticSegmentMembersAddResult result)
        {
            if (result.Errors == null || result.Errors.Count == 0) return;
            foreach (var listError in result.Errors)
                Logger.Warn(string.Format("[AddStaticSegmentMembers error] userId{0}; email:{1}; code:{2}; message:{3};", userId, listError.Email.Email, listError.ErrorCode, listError.ErrorMessage));
        }

        private void HandleBatchSubscribeRejects(int userId, BatchSubscribeResult batchResult)
        {// assuming no errors possible on updated emails (emails that already in mailchimp list) (approved by Serge)
            if (batchResult.ErrorCount == 0) return;
            foreach (var listError in batchResult.Errors)
            {
                try
                {
                    ChimpRejectsRepository.Add(new CHIMP_Rejects
                    {
                        AddOn = DateTime.Now,
                        Email = listError.Email.Email,
                        UserId = userId,
                        Message = string.Format("code:{0}, message:{1}", listError.ErrorCode, listError.ErrorMessage)
                    });
                    string error;
                    ChimpRejectsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
                }
                catch (Exception e)
                {
                    Logger.Error("SaveRejectedEmail", e, CommonEnums.LoggerObjectTypes.Mailchimp);
                }
            }
        }


        private bool ValidateUserCredentials(ChimpUserListDTO token, out string error)
        {
            error = string.Empty;
            try
            {
                var mc = new MailChimpManager(token.ApiKey);
                mc.GetStaticSegmentsForList(token.Uid); // throws if no such list
            }
            catch (Exception e)
            {
                error = FormatError(e);
                return false;
            }
            return true;
        }

        private bool GetChimpKeys(int listId, out CHIMP_UserLists chimpKeysEntity, out string error)
        {
            error = string.Empty;
            chimpKeysEntity = ChimpUserListRepository.GetById(listId);
            if (chimpKeysEntity == null)
            {
                error = "MailChimp Keys not found";
                return false;
            }
            return true;
        }

        #endregion

        public bool GetListSegments(ChimpUserListDTO token)
        {
            token.Segments = ChimpListSegmentRepository.GetMany(x => x.ListId == token.ListId).Select(x => x.Entity2SegmentDto()).ToList();
            return token.Segments.Count > 0;
        }
        public ChimpUserListDTO GetUserListDto(int userId)
        {
            try
            {
                var entityList = ChimpUserListRepository.GetMany(x => x.UserId == userId).ToList();
                return entityList.Count > 0 ? entityList[0].Entity2UserListDto() : null;
            }
            catch (Exception exc)
            {
                Logger.Error("GetUserListDto", exc, CommonEnums.LoggerObjectTypes.Mailchimp);
                return null;
            }
        }

        public bool SaveUserList(ChimpUserListDTO token, out string error)
        {           
            try
            {
                var entityList = ChimpUserListRepository.GetMany(x => x.ApiKey == token.ApiKey && x.Uid == token.Uid).ToList();
                if (entityList.Count == 0)
                {
                    if (!ValidateUserCredentials(token, out error)) 
                        return false;
                    
                    var entity = token.Token2UserListEntity();
                    ChimpUserListRepository.Add(entity);
                    if (!ChimpUserListRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    token.ListId = entity.ListId;

                    return true;
                }
                
                error = "record already exists";
                return false;
            }
            catch (Exception exception)
            {
                error = string.Format("SaveUserList exception::{0}", FormatError(exception));
                Logger.Error("SaveUserList", exception, CommonEnums.LoggerObjectTypes.Mailchimp);
                return false;
            }
        }

        public bool SaveListSubscribers(int listId, out string error)
        {
            CHIMP_UserLists chimpKeysEntity;
            if (!GetChimpKeys(listId, out chimpKeysEntity, out error))
                return false;
           
            var mc = new MailChimpManager(chimpKeysEntity.ApiKey);
            try
            {
                var subscriberList = ChimpUserListRepository.GetAuthorSubscribers(chimpKeysEntity.UserId);
                var emailList = new List<BatchEmailParameter>();
                var count = 0;
                var rejected = 0;
                foreach (var crsLearnerToken in subscriberList)
                {
                    emailList.Add(crsLearnerToken.GetBatchEmailParameter());

                    if ((++count) % MAX_LIST_SUBSCRIBE != 0) continue;

                    var batchResult = mc.BatchSubscribe(chimpKeysEntity.Uid, emailList, false, true); 
                    rejected += batchResult.ErrorCount;
                    HandleBatchSubscribeRejects(chimpKeysEntity.UserId, batchResult);
                    emailList.Clear();
                }
                if (emailList.Count > 0)
                {
                    var batchResult = mc.BatchSubscribe(chimpKeysEntity.Uid, emailList, false, true);
                    rejected += batchResult.ErrorCount;
                    HandleBatchSubscribeRejects(chimpKeysEntity.UserId, batchResult);
                }

                chimpKeysEntity.UpdateUserListEntity(count - rejected);
                ChimpUserListRepository.Update(chimpKeysEntity);
                if (!ChimpUserListRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
            }
            catch (Exception exception)
            {
                error = FormatError(exception);
                Logger.Error("SaveListSubscribers", exception, listId, CommonEnums.LoggerObjectTypes.Mailchimp);
                return false;
            }


            return SaveSegmentSubscribers(listId, out error); 
        }

        private bool SaveSegmentSubscribers(int listId, out string error)
        {
            error = string.Empty;

            CHIMP_UserLists chimpKeysEntity;
            if (!GetChimpKeys(listId, out chimpKeysEntity, out error))
                return false;

            var segmentEntities = ChimpListSegmentRepository.GetMany(x => x.ListId == listId).ToList();
            if (segmentEntities.Count.Equals(0))
            {
                error = "Mailchimp segments not found";
                return false;
            }

            var dateFrom = DateTime.Now.Subtract(new TimeSpan(0, 24, 0, 0));
            var mc = new MailChimpManager(chimpKeysEntity.ApiKey);

            var allSubscribers = ChimpUserListRepository.GetAuthorSubscribers(chimpKeysEntity.UserId);
            try
            {
                foreach (var segEntity in segmentEntities)
                {
                    List<CHIMP_ItemSubscribers> subscribers = null;
                    var emailParams = new List<EmailParameter>();
                    var emailList = new List<string>();
                    var segmentId = Int32.Parse(segEntity.Uid);
                    var itemId = segEntity.BundleId ?? segEntity.CourseId;
                    var itemType = segEntity.CourseId != null ? 1 : 2;

                    mc.ResetStaticSegment(chimpKeysEntity.Uid, segmentId);


                    var segmentType = Utils.ParseEnum<eSegmentTypes>(segEntity.SegmentTypeId);
                    switch (segmentType)
                    {
                        case eSegmentTypes.Active:
                            if (allSubscribers != null)
                                emailList = allSubscribers.Where(x => x.StatusId == (int) BillingEnums.eAccessStatuses.ACTIVE).Select(x => x.Email).ToList();
                            break;
                        case eSegmentTypes.InActive:
                            if (allSubscribers != null)
                                emailList = allSubscribers.Where(x => x.StatusId != (int) BillingEnums.eAccessStatuses.ACTIVE).Select(x => x.Email).ToList();
                            break;
                        case eSegmentTypes.Item:
                            subscribers = ChimpListSegmentRepository.GetItemSubscribers(itemId, itemType);
                            if (subscribers != null)
                                emailList = subscribers.Where(x => x.StatusId == (int)BillingEnums.eAccessStatuses.ACTIVE).Select(x => x.Email).ToList();
                            break;
                        case eSegmentTypes.ItemNew:
                            subscribers = ChimpListSegmentRepository.GetItemSubscribers(itemId, itemType);
                            if (subscribers != null)
                                emailList = subscribers.Where(x => x.StatusId == (int)BillingEnums.eAccessStatuses.ACTIVE && x.AddOn >= dateFrom).Select(x => x.Email).ToList();
                            break;
                    }


                    var count = 0;
                    foreach (var email in emailList)
                    {
                        emailParams.Add(email.GetEmailParameter());

                        if ((++count) % MAX_SEGMENT_SUBSCRIBE != 0) continue;

                        var result = mc.AddStaticSegmentMembers(chimpKeysEntity.Uid, segmentId, emailParams);
                        HandleAddStaticSegmentMembersError(chimpKeysEntity.UserId, result);
                        emailParams.Clear();
                    }
                    if (emailParams.Count > 0)
                    {
                        var result = mc.AddStaticSegmentMembers(chimpKeysEntity.Uid, segmentId, emailParams);
                        HandleAddStaticSegmentMembersError(chimpKeysEntity.UserId, result);
                    }
                    
                    segEntity.UpdateSegemntEntity(emailList.Count);
                    ChimpListSegmentRepository.Update(segEntity);
                    if (!ChimpListSegmentRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                }

            }
            catch (Exception exception)
            {
                error = FormatError(exception);
                Logger.Error("SaveListSubscribers", exception, listId, CommonEnums.LoggerObjectTypes.Mailchimp);
                return false;
            }

            return true;
        }

        public List<ChimpSegmentNameToken> GetMissingSegments(int userId, int listId)
        {
            var tokenList = new List<ChimpSegmentNameToken>();
            var missingSegmentList = ChimpListSegmentRepository.GetMissingSegmentTokens(userId, listId);
            foreach (var missingSegment in missingSegmentList)
            {
                var nameList = missingSegment.ToSegmentNameTokenList();
                tokenList.AddRange(nameList);
            }
            return tokenList;
        }

        public bool SaveListSegment(int listId, out string error)
        {
            error = string.Empty;

            CHIMP_UserLists chimpKeysEntity;
            if (!GetChimpKeys(listId, out chimpKeysEntity, out error))
                return false;

            var mc = new MailChimpManager(chimpKeysEntity.ApiKey);

            var missingSegmentList = ChimpListSegmentRepository.GetMissingSegmentTokens(chimpKeysEntity.UserId, listId);
            
            foreach (var missingSegment in missingSegmentList)
            {
                var nameList = missingSegment.ToSegmentNameTokenList();
                foreach (var nameToken in nameList)
                {
                    try
                    {
                        var mcRes = mc.AddStaticSegment(chimpKeysEntity.Uid, nameToken.Name);
                        nameToken.Uid = mcRes.NewStaticSegmentID.ToString();

                        ChimpListSegmentRepository.Add(missingSegment.Token2SegmentEntity(nameToken));

                        if (!ChimpListSegmentRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                    }
                    catch (Exception mcException)
                    {
                        error = FormatError(mcException);
                        Logger.Error("Save list segments",mcException,listId,CommonEnums.LoggerObjectTypes.Mailchimp);
                        return false;
                    }
                }
            }

            return true;
        }

                
    
    }
   
}


