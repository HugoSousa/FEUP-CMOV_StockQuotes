/** database.js **/

var mysql = require('mysql');
var async = require('async');
var moment = require('moment');


var fs = require('fs');


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
  connection.query('select * from user_share where iduser = ?', [userid], function (err, rows, fields) {
    if (!err){
        cb(null, rows);
      }
      else{
        console.log('Error while performing Query.');
        cb(err, null);
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

exports.updateStatistics = function(employee, body, cb){

  var uploaded_routes = parseInt(body.uploaded_routes);
  var uploaded_tickets = parseInt(body.uploaded_tickets);
  var validated_tickets = parseInt(body.validated_tickets);
  var fraudulent_tickets = parseInt(body.fraudulent_tickets);
  var no_shows = parseInt(body.no_shows);

  connection.query('update employee set uploaded_routes = uploaded_routes + ?, uploaded_tickets = uploaded_tickets + ?, validated_tickets = validated_tickets + ?, fraudulent_tickets = fraudulent_tickets + ?, no_shows = no_shows + ? where idemployee = ?', [uploaded_routes, uploaded_tickets, validated_tickets, fraudulent_tickets, no_shows, employee], function (err, rows, fields) {
    if (!err){
        cb(null, {message: 'Update successful'});
      }
      else{
        console.log('Error while performing Query.');
        cb(err, null);
      }
  });
}