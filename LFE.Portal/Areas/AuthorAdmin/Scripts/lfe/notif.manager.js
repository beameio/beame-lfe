var userMessages = {
    SAVE_YOUR_WORK: 'Save Your work first',    
    CONFIRM_DELETE: 'Delete record?',
    COURSE: {
        COURSE_SAVED: "Course saved successfully"
       ,BUNDLE_SAVED: "Bundle saved successfully"
    },
    WEB_STORE: {
        STORE_SAVED: "Store saved successfully",
        SELECT_CATEGORY:"Select category"
    },
    CHAPTER: {
        SELECT_CHAPTER : 'Select chapter'
    },
    COUPON_PREVENT_DELETE: "Coupon already belong to user and can't before deleted",
    UPLOAD: {
        ONLY_ONE_FILE: "Only one file can be uploaded",
        ONLY_IMAGE: "Only images can be uploaded",
        ONLY_VIDEO: "Only videos can be uploaded",
        VIDEO_SAVED: "Video saved successfully",
        VIDEO_UPDATED: "Changes may take up to 10 minutes to process",
        VIDEO_THUMB_UPDATED: "Video Thumb saved. Changes may take up to 10 minutes to refresh",
        VIDEO_PROGRESS_CONFIRM: "You have active uploads.Close window?",
        VIDEO_PREVENT_DELETE: "Video attached to course and can't be deleted",
        VIDEO_DELETE_CONFIRM:"Delete video?"
    },   
    DISCUSSION: {
        ROOM_SAVED: "ClassRoom Saved",
        ROOM_PREVENT_DELETE:"Room has related courses and can't be deleted"
    }
};

var notifEvents = {
    object: {
        objectSaved: 'object/Saved',
        objectDeleted: 'object/Deleted'
    },
    report: {
        periodChanged: 'period/Changed',
        courseChanged: 'course/Changed',
        reviewBound: 'review/Bound',
        courseBound: 'course/Bound',
        bundleBound: 'bundle/Bound',
        storesBound: 'stores/Bound',
        dashRepKindChanged: 'dashRepKindChanged',
        sortCourse: 'sort/Course',
        sortBundle: 'sort/Bundle',
        salesRequestEnd: 'sales/RequestEnd',
        dateFilterChanged: 'dateFilter/Changed',
        clearFilter: 'clear/Filter',
        rebindReport: 'rebind/Report',
        rebindSalesReport: 'rebind/SalesReport',
        rebindSubscriptionSalesReport: 'rebind/SubscriptionSalesReport',
        oneTimeRepBound: 'oneTimeRep/Bound',
        oneTimeSalesTotal: 'oneTimeSales/Total',
        subscriptionRepBound: 'subscriptionRep/Bound',
        subscriptionSalesTotal: 'subscriptionSales/Total'
    },
    course: {
        courseCreated: 'course/Created',
        bundleCreated: 'bundle/Created',
        videoSelected: 'video/Selected',
        courseStateChanged: 'courseState/Changed',
        priceLinesBound: 'priceLines/Bound',
        pricelessFlagChanged: 'pricelessFlag/Changed',
        coursePriceMetaSaved: 'coursePriceMeta/Saved'
    },
    wizard: {
        'navLoaded': 'navigator/loaded',
        'stateUpdated': 'state/Updated',
        'detailStepSaved': 'detailStep/Saved',
        'stepSaved': 'step/saved',
        'submitWizardStep': 'submit/WizardStep',
        'valueDateSet': 'valueDateSet',
        'stepLoaded': 'step/loaded',
        'rebindTree': 'rebind/Tree',
        'stepsUpdated': 'steps/Updated',
        'chaptersBound': 'chapters/Bound',
        'courseCreated': 'course/Created',
        'chapterChanged': 'chapter/Changed',
        'courseNameChanged': 'courseName/Changed',
        'saveStep': 'save/Step'
    },
    chapter: {
        formLoaded: 'form/Loaded',
        chapterSaved: 'chapter/Saved',
        videoSaved: 'video/Saved',
        linkSaved: 'link/Saved'
    },
    coupon: {
        couponSaved: 'coupon/Saved',
        couponInitEdit: 'coupon/InitEdit',
        switch2List: 'switch/2List'
    },
    video: {
        searchVideos: 'search/Videos',
        videoSaved: 'video/Saved',
        videosLoaded: 'videos/Loaded',
        videosUploaded: 'videos/Uploaded'
    },
    webstore: {
        storeCreated: 'store/Created',
        storeStateChanged: 'storeState/Changed',
        formLoaded: 'form/Loaded',
        categorySaved: 'category/Saved',
        courseSaved: 'course/Saved',
        lfeCategoryAdded: 'lfeCategory/Added',
        authorAdded: 'author/Added'
    },
    discussion: {
        roomSaved: 'room/Saved',
        roomsLoaded: 'rooms/Loaded'
    },
    notif: {
        notifyNewMsg: 'notify/NewMsg',
        userNotifBound: 'userNotif/Bound'
    },
    file: {
        fileUploaded: 'file/Uploaded'
    },
    account: {
        payoutSaved: 'payoutSaved'
    },
    quiz: {
        loadQuizContent: 'load/QuizContent',
        quizStatusChanged:'quizStatus/Changed',
        quizAdd: 'quiz/Add',
        questionAdd: 'question/Add',
        questionAddNew: 'question/AddNew',
        questionEdit: 'question/Edit',
        questionSaved: 'question/Saved',
        questionStateChanged: 'questionState/Changed',
        questionReadEnd:'questionRead/End',
        courseQuizSaved: 'courseQuiz/Saved',
        certificateSaved: 'certificate/Saved'
    }
};

function showNotification(response) {
    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
    var msg = response.success ? response.message : response.error;

    window.formUserNotifManager.show({ message: msg, kind: kind });
}