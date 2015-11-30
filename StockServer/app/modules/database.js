/** database.js **/

var mysql = require('mysql');
var async = require('async');
var moment = require('moment');
var http = require('follow-redirects').http;

var connection = mysql.createConnection({
  host     : 'localhost',
  port     : 3306,
  user     : 'root',
  password : 'admin',
  database : 'new_york_stock_exchange'
});


exports.getAllShares = function ( cb) {

  connection.query('select * from share', [], function (err, rows, fields) {
    if (!err){
        cb(null, rows);
      }
      else{
        console.log('Error while performing Query.');
        cb(err,null);
      }
  });
}


exports.registeruser = function (user, cb) {
//TODO add phone id
  console.log(user);
  connection.query('insert into user(login, password) values (?,?)',[ user.username, user.password], function (err, result) {
    if (!err){
        cb(null,  "Successfully registered user " + result.result );
    }
    else{
      console.log('Error on registration.', err);
      cb(err,null);
    }
  });

         
};

exports.getUserByUsername = function (username, cb) {
  connection.query('select * from user where login = ?',[username], function (err, rows, fields) {
    if (!err){
        cb(null, rows[0]);
      }
      else{
        console.log('Error while performing Query.', err);
        cb(err,null);
      }
  });
}

exports.getPortfolio = function(userid, cb){

  var portfolio_shares = '';
  connection.query('select * from user_share where iduser = ?', [userid], function (err, rows, fields) {
    if (!err){
      async.series([
          function(callback){
            async.forEachOf(rows, function(row, index, callback1){          
                connection.query('select * from share where idshare = ?', [rows[index]['idshare']], function (err1, rows1, fields1) {
                  if (!err1){
                    portfolio_shares += rows1[0]['symbol'] + ',';
                    rows[index]['symbol'] = rows1[0]['symbol'];
                    rows[index]['name'] = rows1[0]['name'];
                  }
                  else{
                    console.log('Error while performing Query.', err1);
                    cb(err1,null);
                  }
                  callback1();
                });
              },
              function(err){
                if(!err){
                  //cb(null, rows);
                  callback(null, 'one');
                }else{
                  cb(err,null);
                }
              }
            );
          },
          function(callback){
            
            portfolio_shares = portfolio_shares.substring(0, portfolio_shares.length - 1);
            var str = '';

            //http request to yahoo finance API
            var options = {
              host: 'finance.yahoo.com',
              path: '/d/quotes?f=sl1d1t1v&s=' + portfolio_shares,
              method: 'GET'
            };

            callback_yahoo = function(response) {
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
                  for(var j = 0; j < rows.length; j++){
                    //console.log("SYMBOL1 " + rows[j]['symbol']);
                    //console.log("SYMBOL2 " + symbol);

                    if(rows[j]['symbol'] == symbol){
                      rows[j]['value'] = value; 
                    }
                  }
                }

                callback(null, 'two');
              });
            }

            var r = http.request(options, callback_yahoo)
            r.on('error', function(error) {
              console.log(error);
            });

            r.end();
          }
      ],
      function(err, results){
        cb(null, rows);
      });
          
    }else{
      cb(err,null);
    }
  });
}

exports.addToPortfolio = function(userid,sharesymbol,cb) {
    connection.query('select * from share where symbol = ?', [sharesymbol], function (err, rows, fields) {
    if (!err){
        if (rows[0] != undefined) {
              var share = rows[0];
             connection.query('insert into user_share(iduser, idshare) values (?,?)',[ userid, share.idshare], function (err, result) {
                if (!err){
                    cb(null,  "Successfully added new share");
                }
                else{
                  console.log('Error on share addition: ', err);
                  cb(err,null);
                }
              });
        }
        else cb("Share not found", null);
      }
      else{
        console.log('Error while performing search.');
        cb(err, null);
      }
  });
}

exports.setFavoriteShare = function(userid,sharesymbol,cb) {
    connection.query('select * from share where symbol = ?', [sharesymbol], function (err, rows, fields) {
    if (!err){
        if (rows[0] != undefined) {
              var share = rows[0];
              var shareid = share.idshare;
              connection.query('UPDATE user SET main_share= ? WHERE iduser = ?',[shareid, sharesymbol], function(err, rows, fields) {
                 if (!err){
                    cb(null,  "Successfully starred share " + sharesymbol);
                }
                  else{
                  console.log('Error on share starring: ', err);
                  cb(err,null);
                }
              })
        }
        else cb("Share not found", null);
      }
      else{
        console.log('Error while performing search.');
        cb(err, null);
      }
  });
}

exports.setLimits = function(userid, sharesymbol , limit_up, limit_down,cb) {
    connection.query('SELECT * FROM user_share us join share s on us.idshare = s.idshare where us.iduser = ? and s.symbol = ?', [userid, sharesymbol], function (err, rows, fields) {
    if (!err){
        if (rows[0] != undefined) {
              var share = rows[0];
              var shareid = share.idshare;
              var mapid = share.iduser_share;

              connection.query('UPDATE user_share SET limit_up = ? , limit_down = ? WHERE iduser_share = ?', [limit_up, limit_down, mapid], function(err, rows, fields) {
                   if (!err){
                    cb(null,  "Successfully updated limits of share " + sharesymbol);
                }
                  else{
                  console.log('Error on share limit updating: ', err);
                  cb(err,null);
                } 
              });
        }
        else cb("Share not found in portfolio", null);
      }
      else{
        console.log('Error while performing search.');
        cb(err, null);
      }
  });
}

exports.getShare = function(userid, sharesymbol, cb) {
    connection.query('SELECT * FROM user_share us join share s on us.idshare = s.idshare where us.iduser = ? and s.symbol = ?', [userid, sharesymbol], function (err, rows, fields) {
    if (!err){
      //cb(null,rows[0]);
      //get the value of the share
      var str = '';

      //http request to yahoo finance API
      var options = {
        host: 'finance.yahoo.com',
        path: '/d/quotes?f=sl1d1t1v&s=' + sharesymbol,
        method: 'GET'
      };

      callback_yahoo = function(response) {
        //another chunk of data has been recieved, so append it to `str`
        response.on('data', function (chunk) {
          str += chunk;
        });

        //the whole response has been received, so we just print it out here
        response.on('end', function () {
          //console.log(str);
          var line = str.split('\n')[0];
          line_fields = line.split(",");
          symbol = line_fields[0].replace(/\"/g, "");
          var date = line_fields[2].replace(/\"/g, "");
          var time = line_fields[3].replace(/\"/g, "");

          value = parseFloat(line_fields[1]);
          rows[0]['value'] = value; 
          rows[0]['date'] = date;
          rows[0]['time'] = time;

          cb(null, rows[0]);
        });
      }

      var r = http.request(options, callback_yahoo)
      r.on('error', function(error) {
        console.log(error);
      });

      r.end();
    }
    else{
      console.log('Error while performing search.');
      cb(err, null);
    }
  });
}

exports.getShareEvolution = function (symbol, start, end, cb) {

  var result = [];

  var a = start.month();
  var b = start.date();
  var c = start.year();
  var d = end.month();
  var e = end.date();
  var f = end.year();
  var g = 'd'; //hardcoded daily for now

  var request_string = "a=" + a + "&b=" + b + "&c=" + c + "&d=" + d + "&e=" + e + "&f=" + f + "&g=" + g + "&s=" + symbol;
  console.log("REQUEST : " + request_string);

  //http request to yahoo finance API
  var options = {
    host: 'ichart.finance.yahoo.com',
    path: '/table.txt?' + request_string,
    method: 'GET'
  };

  callback_yahoo = function(response) {
    var str = '';
    //another chunk of data has been recieved, so append it to `str`
    response.on('data', function (chunk) {
      str += chunk;
    });

    //the whole response has been received, so we just print it out here
    response.on('end', function () {
      //console.log(str);
      var lines = str.split('\n');
      for(var i = 1; i < lines.length - 1; i++){
        //date, open, high, low, close, volume, adj close
        line_fields = lines[i].split(",");
        console.log(lines[i]);

        var date = line_fields[0];
        var high = parseFloat(line_fields[2]);
        var low = parseFloat(line_fields[3]);

        var result_point = {
          date: date,
          high: high,
          low: low
        }

        result.push(result_point);
      }
      cb(null, result);
    });
  }

  var r = http.request(options, callback_yahoo)
  r.on('error', function(error) {
    console.log(error);
    cb(err, null);
  });

  r.end();
}
