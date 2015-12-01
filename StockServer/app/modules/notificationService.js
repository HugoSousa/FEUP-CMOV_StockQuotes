var http = require('follow-redirects').http;
var wns = require('wns');

// check if an element exists in array using a comparer function
// comparer : function(currentElement)
Array.prototype.inArray = function(comparer) { 
    for(var i=0; i < this.length; i++) { 
        if(comparer(this[i])) return true; 
    }
    return false; 
}; 

// adds an element to the array if it does not already exist using a comparer 
// function
Array.prototype.pushIfNotExist = function(element, comparer) { 
    if (!this.inArray(comparer)) {
        this.push(element);
    }
}; 


module.exports = function (database) {
	var cache = {
		data: [],
		database: database
	};

	var state = function () {};

	state.prototype.prepCache = function (callback) {
		setInterval(function () {
			processNotifications(database);
		}, 5000);
	};

	state.prototype.manualNotify = function (client, text, callback) {
		//external stub
		notifyManual(client, text, callback);
	};

	var notifyManual = function (client, text, callback) {
		 callback(null, "Client not subscribed");
	}

	var processNotifications = function(database) {
		var sharesArray = {};

		database.getAllShares(function(error, result) {
			if (error) console.log("Failure retrieving all shares on NS");
			else {
				result.forEach(function(value) {
					sharesArray[value['idshare']] = {symbol: value['symbol'], name: value['name']};
				});

				database.getRelevantShares(function(error, result) {
					var user_shares = result;
					var query_shares = [];
					result.forEach(function(row) {
					if (row['limit_up'] === parseFloat(row['limit_up'], 10) || row['limit_down'] === parseFloat(row['limit_down'], 10)) {
						// is relevant
						query_shares[row.idshare] = sharesArray[row.idshare];


					}
					if (row['main_share'] === parseFloat(row['main_share'], 10)) {
						// is main_share
						query_shares[row.idshare] = sharesArray[row.idshare];
					}
					});
					requestData(database, query_shares, user_shares, sharesArray, function(err, result) {
						if (err)
							console.log(err);
						else
							notify(result);
					});
				});

			}
		});
	};

	var requestData = function(database, query_shares, user_shares, sharesArray, cb) { 
		var portfolio_shares = "";

		query_shares.forEach(function(value) {
			 portfolio_shares += value['symbol'] + ',';
		});


	    portfolio_shares = portfolio_shares.substring(0, portfolio_shares.length - 1);
        //http request to yahoo finance API
        var options = {
          host: 'finance.yahoo.com',
          path: '/d/quotes?f=sl1d1t1v&s=' + portfolio_shares,
          method: 'GET'
        };

       var str = "";
       var callback_yahoo = function(response) {
          //another chunk of data has been recieved, so append it to `str`
          response.on('data', function (chunk) {
            str += chunk;
          });

          //the whole response has been received, so we just print it out here
          response.on('end', function () {
            //console.log(str);
            var lines = str.split('\n');
            for(var i = 0; i < lines.length - 1; i++){

              line_fields = lines[i].split(",");
              symbol = line_fields[0].replace(/\"/g, "");
              value = parseFloat(line_fields[1]);
              //console.log(symbol + " / " + value);
              update_data(query_shares, symbol, value);
            }
            prepare_notifications(user_shares, query_shares, function(err, result) {
            	cb(err, result);
            })
          });
        }

        var r = http.request(options, callback_yahoo)
        r.on('error', function(error) {
          console.log(error);
        });

        r.end(); 
	};

	var update_data = function(array, symbol, value) {
		array.forEach(function(share, index) {
			if (share.symbol == symbol) array[index].value = value;
		})
	}


	var prepare_notifications = function(user_shares, shares_info, cb) {
		var notifications = [];
		user_shares.forEach(function(row) {
		var share = shares_info[row.idshare];

		if (row['limit_up'] === parseFloat(row['limit_up'], 10) || row['limit_down'] === parseFloat(row['limit_down'], 10)) {
			// is relevant
			if (share.value >= row['limit_up'] && row['limit_up'] === parseFloat(row['limit_up'], 10)) {
				notifications.pushIfNotExist({
					user: row.iduser,
					username: row.login,
					type: "upper",
					symbol: share.symbol,
					name: share.name,
					value: share.value,
					limit: row['limit_up']

				}, function(e) { return e.user === row.iduser && e.type == 'upper' && e.symbol == share.symbol});
			} 

			if (share.value <= row['limit_down'] && row['limit_down'] === parseFloat(row['limit_down'], 10)) {
				notifications.pushIfNotExist({
					user: row.iduser,
					username: row.login,
					type: "lower",
					symbol: share.symbol,
					name: share.name,
					value: share.value,
					limit: row['limit_down']

				}, function(e) { return e.user === row.iduser && e.type == 'lower'});

			}

		}

		share = shares_info[row.main_share];
		if (row['main_share'] === parseFloat(row['main_share'], 10)) {
			//TODO add user phone id
			notifications.pushIfNotExist({
				user: row.iduser,
				username: row.login,
				type: "star",
				symbol: share.symbol,
				name: share.name,
				value: share.value

			}, function(e) { return e.user === row.iduser && e.type === "star" && e.symbol == share.symbol } );
		}
		});
		cb(null, notifications);
	};


	var notify = function(notifications) {
			//console.log(notifications);
		notifications.forEach(function(notification) {
			//console.log(notification);
			var channelUrl = '{url to your application notification channel}';
			var options = {
			    client_id: '000000004C1727C9',
			    client_secret: 'ivJo7vftYrwIoD4M4DgfYyegrztbtLja'   
			};
			/*
			wns.sendTileSquareBlock(channelUrl, 'Yes!', 'It worked!', options, function (error, result) {
			    if (error)
			        console.error(error);
			    else
			        console.log(result);
			});
*/
		});

	}
	return state;

}
