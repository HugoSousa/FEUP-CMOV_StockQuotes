var http = require('http');

module.exports = function (database) {
	var cache = {
		data: []
	};

	var state = function () {};

	state.prototype.prepCache = function (callback) {
		setInterval(function () {
			console.log("check");
			//clearVisits();
			//clearPositions();
		}, 1000);
	};

	state.prototype.manualNotify = function (client, text, callback) {
		//external stub
		notifyManual(client, text, callback);
	};

	var notifyManual = function (client, text, callback) {

		 callback(null, "Client not subscribed");


	}

	return state;

}
