var userMessages = {
    REVIEW_SAVED: 'Review saved',
    MESSAGE_SAVE: "Message saved",
    INVALID_MESSAGE: "Message invalid or empty"
};

var notifEvents = {
    window: {
        windowResized: 'window/Resized'
    },
    object: {
        objectSaved: 'object/Saved',
        objectDeleted: 'object/Deleted'
    },   
    course: {
        courseSelected: 'course/Selected',
        ratingUpdated: 'rating/Updated',
        saveState: 'saveState',
        listBound: 'courselist/Bound'
    },
    chapter: {
        chapterSelected: 'chapter/Selected',
        chapterChanged: 'chapter/Changed'
    },
    video: {
        videoSelected: 'video/Selected',
        videoChanged: 'video/Changed'
    },
    report: {
        periodChanged: 'period/Changed',
        courseBound: 'course/Bound',
        sortCourse: 'sort/Course',
        oneTimeRepBound: 'oneTimeRep/Bound',
        subscriptionRepBound: 'subscriptionRep/Bound'
    },
    disqus: {
         rebindFeed: "rebind/Feed"
        ,showFeed: 'show/Feed'
        , messageClicked: 'message/Clicked'         
    },
    notif: {
        notifyNewMsg: 'notify/NewMsg',
        userNotifBound: 'userNotif/Bound'
    },
    file: {
        fileUploaded: 'file/Uploaded'
    }
};
