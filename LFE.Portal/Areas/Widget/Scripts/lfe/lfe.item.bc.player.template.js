function markup(html, data) {
    var m;
    var i = 0;
    var match = html.match(data instanceof Array ? /{{\d+}}/g : /{{\w+}}/g) || [];

    while (m = match[i++]) {
        html = html.replace(m, data[m.substr(2, m.length - 4)]);
    }
    return html;
}

var playerTemplate =
    "<div style=\"display:none\"></div>" +
        "<object id=\"{{objectId}}\" class=\"BrightcoveExperience\">" +
        "<param name=\"bgcolor\" value=\"#fff\" />" +
        //"<param name=\"autoStart\" value=\"{{autoStart}}\" />" +
        //"<param name=\"width\" value=\"{{width}}\" />" +
        //"<param name=\"height\" value=\"{{height}}\" />" +
        "<param name=\"playerID\" value=\"1775581216001\" />" +
        "<param name=\"playerKey\" value=\"AQ~~,AAABm0drRPk~,DiQRmh9VgVWZubcHWonC2cZbcR-19kzC\" />" +
        "<param name=\"isVid\" value=\"true\" />" +
        "<param name=\"isUI\" value=\"true\" />" +
        "<param name=\"wmode\" value=\"opaque\" />" +        
        "<param name=\"dynamicStreaming\" value=\"true\" />" +
        "<param name=\"htmlFallback\" value=\"true\" />" +
        "<param name=\"includeAPI\" value=\"true\" />" +
        "<param name=\"@videoPlayer\" value=\"{{videoID}}\" />" +
        "<param name=\"templateLoadHandler\" value=\"bcLfePlayer.onTemplateLoaded\" />" +
        "<param name=\"templateReadyHandler\" value=\"bcLfePlayer.onTemplateReadyHandler\" />" +
        //"<param name=\"secureConnections\" value=\"{{isSecured}}\" />" +
        //"<param name=\"secureHTMLConnections\" value=\"{{isSecured}}\" />" +
    "</object>";