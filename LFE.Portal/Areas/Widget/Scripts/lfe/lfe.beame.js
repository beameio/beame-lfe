var mobileProdData;

function onProductLoaded(data) {

    mobileProdData = data;
//    $.ajax({
//        url: '/Beame/Viewer',
//        cache: false,
//        type: "Post",
//        async:true,
//        datatype: "json",
//        contentType: "application/json; charset=utf-8"
//		, success: function (view) {
//		    $('body').empty().html(view);
//		    setTimeout(window.hideLoader, 150);
//		}
//    });
    $('body').empty();
	$('<iframe>', {
		src: '/Beame/Viewer',
		id: 'ifrm-viewer',
		class:'full-screen',
		frameborder: 0
	}).appendTo('body');
}
function onDisconnect() {
	console.log('disconnected');
}

function onDeviceDisconnected() {
	console.log('onDeviceDisconnected');
	// location.reload();
}

function onDeviceDescriptor(device) {
	//here will received callback from mobile
	console.log('onDeviceDescriptor ---', device);
	device.receivePairedCmd('HUY^2');
}

function onError() {
	console.log('disconnected');
}

function getMobileData() {
    return mobileProdData;
}

function onRoutes(routesDescriptor) {

	console.log('routesDescriptor is', routesDescriptor);

	var data = window.location + '?d=' +
		encodeURIComponent(
			JSON.stringify(
				luckyNetwork.device().getRoutesDescriptor()
			)
		);//JSON.stringify(routesDescriptor));
	console.log('Connection string', decodeURIComponent(data));

	var qrData = Encoder.htmlDecode(data + window.qrUrl);

	console.log('qr data ---', qrData);

	$('#d-prod-qr').kendoQRCode({
		value: qrData,
		size: 200,
		color: "#000",
		background: "#ffa800"
	});

	luckyNetwork.device().setFunction('onConnect', onMobileConnected);
	luckyNetwork.device().setFunction('onProductLoaded', onProductLoaded);
}



function onMobileConnected(data) {
	console.log('onMobileConnected', data);
}

var luckyNetwork;

$(document).ready(function () {

    if (typeof LuckYNetwork == "undefined") return;

    luckyNetwork = new LuckYNetwork('b4934eba-b275-4016-9d36-2770057f9839');

    luckyNetwork.setCallbacks({
        onRoutes: onRoutes,
        onDeviceDisconnected: onDeviceDisconnected,
        onDeviceDescriptor: onDeviceDescriptor,
        onDisconnect: onDisconnect,
        onError: onError
    });

	$.ajax({
	    url: 'https://prod.luckyqr.io/create-network',
		cache: false,
		type: "Get",
		datatype: "json",
		contentType: "application/json; charset=utf-8"
		, success: function (data) {
			luckyNetwork.connect(data.endpoints, data.endpoints[0], null, function () {
				console.log('on create network', arguments);
				luckyNetwork.requestRoutes();
			});
		}
	});
});