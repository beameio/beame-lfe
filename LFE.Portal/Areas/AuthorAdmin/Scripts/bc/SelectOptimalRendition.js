/**
* Handles always selecting the optimal rendition based on bandwidth and video screen size
* when the player determines a new rendition can be played.
*/

var _player;
var _experienceModule;
// How far in percent a rendition can be above the video screen height.
var FRAME_HEIGHT_TOLERANCE = 1.2;
// The percent amount the detected bandwidth is reduced when checking against encoding.
// Generally, you want a rendition to be encoded less than the available bandwidth in order to 
// ensure smooth playback.
var ENCODING_RATE_TOLERANCE = 0.70;

/**
* Event handler for when the player the player first loads.
*
* @param  id  The HTML ID of the player.
*/
function onTemplateLoaded(id) {
    _player = brightcove.getExperience(id);
    _experienceModule = _player.getModule("experience");
    _experienceModule.addEventListener("templateReady", onTemplateReady);
	var videoPlayer = _player.getModule("videoPlayer");
    if (videoPlayer) {
        // set callback function for rendition selection
        videoPlayer.setRenditionSelectionCallback(selectRendition);
    }
}

/**
* Event handler for when the player is ready for interaction.
*
* @param  event  Event dispatched by player experience module.
*/
function onTemplateReady(event) {
    _experienceModule.removeEventListener("templateReady", onTemplateReady);
}

/**
* The callback invoked whenever the player reaches a point when a new selection can be selected. This will occur on initial playback
* as well, for streaming videos, when there are multiple buffering events or when the screen size changed, as when going full screen.
* This method must take an object as an argument and return an int value that represents the index of the rendition to play.
* If the renditionIndex value returned is -1, or any other value that doesn't correspond to the index of an available rendition, the
* player recalculates which rendition to use, using the normal selection algorithm.
*
* @param  context  The context that the player uses to select a new rendition. This object includes the following properties:
*           video  The video currently playing to which the renditions belong.
*           currentRendition  The currently selected rendition for the video.
*           renditions  An Array of renditions for the video to choose from.
*           detectedBandwidth  The last detected bandwidth value.
*           screenWidth  The pixel width of the video screen in which the rendition will play.
*           screenHeight  The pixel height of the video screen in which the rendition will play.
*
* @returns  The index of the rendition in the renditions list for the video player to play.
*/
function selectRendition(context) {
    var renditions = context.renditions.slice();
    var heightMatch;
    var currentRendition;
    var selectedRendition;
    var bandwidth = context.detectedBandwidth * 1000;
    //console.log('bandwidth:' + bandwidth);
    var height = context.screenHeight;
    for (var i = 0; i < renditions.length; i++) {
        currentRendition = renditions[i];
        if (isNaN(currentRendition.frameHeight)) continue;
        if (isNaN(currentRendition.encodingRate) && bandwidth > -1) continue;
        // bandwidth hasn't yet need detected, look at height
        if (bandwidth == -1) {
            // set the rendition that most closely matches screen height
            if (heightMatch == null
                || Math.abs(currentRendition.frameHeight-height) < Math.abs(heightMatch.frameHeight-height)
            ) {
                heightMatch = currentRendition;
            }
        // if bandwidth has been detected, look at encoding rate;
        // only look at renditions that have encoding rate and height that is less than
        // the current bandwidth and screen height, within a tolerance setting
        } else if (bandwidth > -1
                    && currentRendition.encodingRate <= bandwidth*ENCODING_RATE_TOLERANCE
                    && currentRendition.frameHeight <= height*FRAME_HEIGHT_TOLERANCE
        ) {
            // check whether current rendition being evaluated is closer match than last one assigned to selectedRendition
            if (selectedRendition != null) {
                // if this rendition is closer than previous assigned, use it
                if (Math.abs(currentRendition.frameHeight-height) < Math.abs(selectedRendition.frameHeight-height)) {
                    selectedRendition = currentRendition;
                } else {
                    // since renditions are sorted descending, we will not get a closer match than the current
                    break;
                }
            } else {
                selectedRendition = currentRendition;
            }
        }
    }
    // if we couldn't find anything under the current bandwidth and size...
    if (selectedRendition == null) {
        // bandwidth has been detected, so find the closest encoding
        if (bandwidth > -1) {
            // sort renditions first by encoding, then by height
            renditions = sortRenditions(renditions);
            var bandwidthMatch;
            for (var i = 0; i < renditions.length; i++) {
                currentRendition = renditions[i];
                if (isNaN(currentRendition.encodingRate)) continue;
                if (bandwidthMatch == null) {
                    bandwidthMatch = currentRendition;
                // if this rendition is closer in desired encoding than the previous, use it
                } else if (Math.abs(currentRendition.encodingRate-bandwidth*ENCODING_RATE_TOLERANCE) < Math.abs(bandwidthMatch.encodingRate-bandwidth*ENCODING_RATE_TOLERANCE)) {
                    bandwidthMatch = currentRendition;
                }
            }
            selectedRendition = bandwidthMatch;
        } else if (heightMatch != null) {
            // just grab the closest in height since we have no bandwidth data
            selectedRendition = heightMatch;
        }
    }
    // find the index in the initial list
    var renditionIndex = -1;
    for (var i = 0; i < context.renditions.length; i++) {
        if (selectedRendition == context.renditions[i]) {
            renditionIndex = i;
            break;
        }
    }
    describeRendition(context.renditions[renditionIndex]);
    return renditionIndex;
}

/**
* Sorts renditions first by bandwidth, then by frame height.
*
* @param  renditions  The list of renditions to sort.
*
* @returns  The sorted renditions in a new array.
*/
function sortRenditions(renditions) {
    // first, sort by encoding
    renditions.sort(sortByEncoding);
    // run through sorted list and place same encodings into their own arrays
    var encodingRenditions = {};
    for (var i = 0; i < renditions.length; i++) {
        if (encodingRenditions[renditions[i].encodingRate] == null) {
            encodingRenditions[renditions[i].encodingRate] = [];
        }
        encodingRenditions[renditions[i].encodingRate].push(renditions[i]);
    }
    // now sort all nested arrays and put them back into single array
    var sortedRenditions = [];
    for (var i in encodingRenditions) {
        encodingRenditions[i].sort(sortByFrameHeight);
        sortedRenditions = sortedRenditions.concat(encodingRenditions[i]);
    }
    return sortedRenditions;
}

/**
* Sorts two renditions by encoding rate value.
*/
function sortByEncoding(a, b) {
    var x = a.encodingRate;
    var y = b.encodingRate;
    return ((x < y) ? 1 : ((x > y) ? -1 : 0));
}

/**
* Sorts two renditions by frame height value.
*/
function sortByFrameHeight(a, b) {
    var x = a.frameHeight;
    var y = b.frameHeight;
    return ((x < y) ? 1 : ((x > y) ? -1 : 0));
}

/**
* Traces out the values of rendition for testing.
*
* @param  rendition  The rendition to traces values for.
* @param  index  The index in the renditions array where this rendition is found.
*/
function describeRendition(rendition) {
    var message = ("size: " + rendition.size);
    message += ("\nframeWidth: " + rendition.frameWidth);
    message += ("\nframeHeight: " + rendition.frameHeight);
    message += ("\nencodingRate: " + rendition.encodingRate);
    if (window.console)  console.log(message);
}