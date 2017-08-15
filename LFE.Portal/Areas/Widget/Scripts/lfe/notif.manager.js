var userMessages = {
   REVIEW_SAVED :'Review saved'
};

var notifEvents = {
    object: {
        objectSaved: 'object/Saved',
        objectDeleted: 'object/Deleted',
        openLogin: 'open/Login',
        addressFormLoaded: 'addressForm/Loaded'
    },   
    course: {
        courseSelected: 'course/Selected',
        ratingUpdated: 'rating/Updated',
        saveState: 'saveState'
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
        sortCourse: 'sort/Course'        
    },
    item: {
        priceSelected: 'price/Selected'
    },
    disqus: {
        rebindFeed: "rebind/Feed"
       , showFeed: 'show/Feed'
       , messageClicked: 'message/Clicked'
    },
    notif: {
        notifyNewMsg: 'notify/NewMsg',
        userNotifBound: 'userNotif/Bound'
    },
    window: {
        windowResized: 'window/Resized',
        windowUnload: 'window/Unload',
        ajaxComplete:'ajax/Complete',
        heightChanged: 'height/Changed',
        contentLoaded: 'content/Loaded'
    },    
    quiz: {
        quizSuccess: 'quizSuccess'
    }
};
