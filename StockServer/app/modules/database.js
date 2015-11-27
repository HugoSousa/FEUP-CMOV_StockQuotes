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



//returns the index of central_station_times where there is the less waiting time
getLessWaitingTime = function (central_station_times, last_time) {
  var less_waiting = '23:59:00';
  var index = -1;

  for(var i = 0; i < central_station_times.length; i++){

    var time = central_station_times[i].first_station_time;

    if(time > last_time && time < less_waiting){
      less_waiting = time;
      index = i;
    }
  }

  return index;
}

exports.checkTrainCapacity = function(from, to, train_id, cb){

  var capacity;

  connection.query('select * from train where id = ?', [train_id], function (err, rows, fields) {
      if(!err){
        capacity = rows[0].capacity;
        console.log(rows[0].capacity);
        console.log("CAPACITY: ", capacity);

        //get the affected routes from routes_affectance. 
        //for each affected route, get the other routes with that affected route
        //get the sold tickets for those routes and check if the sum exceeds the train capacity
        //set the sold_out parameter in result
        var affects = [];

        for(var i = 0; i < route_affectances.length; i++){
          if(route_affectances[i].route == (stations[from] + "-" + stations[to])){
            var affected_routes = route_affectances[i].affects;

            for(var j = 0; j < affected_routes.length; j++){

              var affected_ticket_routes = [];

              for(var k = 0; k < route_affectances.length; k++){

                if(route_affectances[k].affects.indexOf(affected_routes[j]) != -1){
                  affected_ticket_routes.push({route_name: route_affectances[k].route});

                }
              }

              if(affected_ticket_routes.length > 0){

                for(var l = 0; l < affected_ticket_routes.length; l++){

                  var route_split = affected_ticket_routes[l].route_name.split("-");
                  var from_station_name = route_split[0];
                  var to_station_name = route_split[1];

                  affected_ticket_routes[l]["from"] = stations.indexOf(from_station_name);
                  affected_ticket_routes[l]["to"] = stations.indexOf(to_station_name);
                }

                affects.push({route: affected_routes[j], affected: affected_ticket_routes });
              }
            }

            break;
          }
        }
        affects['capacity'] = capacity;
        console.log("AFFECTS: " + JSON.stringify(affects));
        cb(null, affects);
        /*
        for(var i = 0; i < affects.length; i++){

          //TODO: change user
          //TODO change date
          var sql_query = "select count(*) as occupied from ticket t join route r join station_stop ss on t.route_id = r.id and ss.route_id = r.id and ss.time = time(t.route_date) where user_id = 1 and is_validated = (0) and date(t.route_date) = '2015-10-18' and time(t.route_date) = ss.time"; 
          
          for( var j = 0; j < affects[i].affected.length; j++){
            
            if(j != 0)
              sql_query += " or";
            else
              sql_query += " and (";

            sql_query += " (r.start_station = " + affects[i].affected[j].from + " and r.end_station = " + affects[i].affected[j].to + " )";
          }

          sql_query += " )";

          //console.log("QUERY: " + sql_query);
          
          connection.query(sql_query, function (err, rows, fields) {
            if(!err){
              console.log("CAPACITY: " + capacity);
              console.log("OCCUPIED: " + rows[0].occupied);
              if(parseInt(rows[0].occupied) >= parseInt(capacity)){
                console.log("RETURNED TRUE");
                cb({index: index, result: true});
              }
              else{
                cb({index: index, result: false});
              }
            }
            else{
              console.log("Error in query: ", err);
            }
          });

        }
        */
      }
      else{
        console.log("Error in query: ", err);
      }
    });
}


exports.getStations = function (cb) {

  connection.query('select * from station', function (err, rows, fields) {
    if (!err){
        cb(null, rows);
      }
      else{
        console.log('Error while performing Query.');
        cb(err,null);
      }
  });
}

exports.getSimpleTrains = function(cb) {
  var simpleTrains = [];
  simple_routes.forEach(function(value, index) {
    var route = {};
    route.start = value.start;
    route.end = value.end;
    route.start_id = stations.indexOf(route.start);
    route.end_id = stations.indexOf(route.end);
    simpleTrains.push(route);
  });


  async.each(simpleTrains, function(train, callback) {

        exports.getTrainTimes(train.start_id, train.end_id, null, function(err, data){
          if (err) {
              console.log("ERROR : ",err);            
          } else { 
              var trips = [];
              data.trips.forEach(function(trip,ind) {
                trips.push( {
                  start_time: trip.times[0],
                  end_time: trip.times[trip.times.length-1],
                  train: trip.train
                });
              })           
              train.trips = trips;   
          }
          callback();    
    });
    
  },
  function (err) {
    cb(null, simpleTrains);
  }
  );
 
}

function getRoutePossibilites(from, to, time, cb) {
   exports.getTrainTimes(from, to, null, function(err, data){
      if (err) {
              cb(null,err);          
          } else { 
            var found = false;
            data.trips.forEach(function(trip, index) {
              //console.log(trip.times[0]);
              //console.log(time);
              if (trip.times[0].toString() == time.toString()) found = trip;
               
            });
            if (!found) cb('Couldnt find trip',null);
            else  {
              var combo_array = [];
              found.stations.forEach(function(station, index, array) {
                //console.log(station);
                for (var i = index +1; i < array.length; i++) {
                  combo_array.push({
                    start: station,
                    end: array[i],
                    time: found.times[index]
                  })
                }
              });
              cb(null,combo_array);
            }
          }
    });
}

exports.getAllTickets = function(from,to,time,date,cb) {
 exports.getSimpleTrains(function(err, simpleTrains){
    if (err) {
            cb(err,null);          
        } else {       
            var found = false;
            simpleTrains.forEach(function (train, index) {
              if (train.start_id == from && train.end_id == to) {
                train.trips.forEach(function(trip) {
                  if (trip.start_time == time) {
                    found = true;
                  }
                });
              }
            });
            if (!found) cb('Unrecognized trip', null);
            else {
              getRoutePossibilites(from, to, time, function(err, combo_array) {
                  if (err) cb(err, null);
                  else {
                    console.log(combo_array);
                    var download_tickets = [];
                   async.each(combo_array, function(combo, callback) {

                      connection.query(
                        'select * from ticket, route where ticket.route_id = route.id and ticket.route_date = ? and route.start_station = ? and route.end_station = ?',
                        [date + ' ' + combo.time, combo.start, combo.end],
                        function (err, rows, fields) {
                          console.log(date + ' ' + combo.time);
                          console.log(rows);
                          for (var i = 0; i < rows.length; i++) {
                            //rows[i].uuid = Array.prototype.slice.call(rows[i].uuid, 0);
                            rows[i].is_validated = Array.prototype.slice.call(rows[i].is_validated, 0);
                            //rows[i].signature = Array.prototype.slice.call(rows[i].signature, 0);
                            rows[i].switch_central = Array.prototype.slice.call(rows[i].switch_central, 0);


                            download_tickets.push(rows[i]);
                          }
                        callback(); 
                        });
                    },
                    function (err) {
                      cb(null, download_tickets);
                    }
                    );
                   // select * from ticket, route where ticket.route_id = route.id and ticket.route_date = '2015-10-29 14:00:00' and route.start_station = 1 and route.end_station = 3;
                  }
              });
            }
        }  
  });
}
exports.getRoute = function (from, to, time, date, cb) {

  //se switch_central = 1, tem de buscar de from -> 3 + 3 -> to
  connection.query('select * from route r where r.start_station = ? and r.end_station = ?', [from, to], function (err0, rows0, fields0) {
    if(!err0){

      if(rows0[0]['switch_central'][0] == 0)
      {
        connection.query('select ss.id, ss.station_id, ss.time, ss.order, ss.train_id from route r join station_stop ss on ss.route_id = r.id where r.start_station = ? and r.end_station = ? and ss.time >= ?', [from, to, time], function (err, rows, fields) {
      
        if (!err){

          var max_order = 1;
          var start = 0; //for cases when the first returned rows don't start in order 1

          for(var i = 0; i < rows.length; i++){
            if(rows[i].order > 1)
              start = i+1;
            else
              break;
          }

          for(var i = 0; i < rows.length; i++){
            if(rows[i].order > max_order)
              max_order = rows[i].order;
          }

          //limit to the first max_order rows
          var result = [];
          for(var i = start; i-start < max_order; i++){
            result.push(rows[i]);
          }

          var route = rows0[0]['id'];

          cb(null, {ticket_1: result, ticket_2: null, route_1: route, route_2: null, price: rows0[0]['price'], distance: rows0[0]['distance'], from: from, to: to, time: time, date: date, sold_out: false});
        }

        else{
          console.log('Error while performing Query 1.');
          cb(err,null);
        }

        });
      }else{
        //necessario retornar bilhete de from->3 e 3->to
          var route1 = rows0[0]['route_1'];
          var route2 = rows0[0]['route_2'];
          var result = {ticket_1: null, ticket_2: null, route_1: route1, route_2: route2, price: rows0[0]['price'], distance: rows0[0]['distance'], from: from, to: to, time: time, date: date, sold_out: false};

          connection.query('select ss.id, ss.station_id, ss.time, ss.order, ss.train_id from route r join station_stop ss on ss.route_id = r.id where r.start_station = ? and r.end_station = 3 and ss.time >= ? ', [from, time], function (err1, rows1, fields1) {
          
            if (!err1){
              var max_order = 1;
              var start = 0; //for cases when the first returned rows don't start in order 1

              for(var i = 0; i < rows1.length; i++){
                if(rows1[i].order > 1)
                  start = i+1;
                else
                  break;
              }

              for(var i = 0; i < rows1.length; i++){
                if(rows1[i].order > max_order)
                  max_order = rows1[i].order;
              }

              //limit to the first max_order rows
              var ticket_1 = [];
              for(var i = start; i-start < max_order; i++){
                ticket_1.push(rows1[i]);
              }

              result.ticket_1 = ticket_1;

              console.log("AQUI!" + ticket_1[ticket_1.length - 1].time);

              connection.query('select ss.id, ss.station_id, ss.time, ss.order, ss.train_id from route r join station_stop ss on ss.route_id = r.id where r.start_station = 3 and r.end_station = ? and ss.time >= ?', [to, ticket_1[ticket_1.length - 1].time], function (err2, rows2, fields2) {
                if(!err2){
                  var max_order = 1;
                  var start = 0; //for cases when the first returned rows don't start in order 1

                  for(var i = 0; i < rows2.length; i++){
                    if(rows2[i].order > 1)
                      start = i+1;
                    else
                      break;
                  }

                  for(var i = 0; i < rows2.length; i++){
                    if(rows2[i].order > max_order)
                      max_order = rows2[i].order;
                  }

                  //limit to the first max_order rows
                  var ticket_2 = [];

                  for(var i = start; i-start < max_order; i++){
                    ticket_2.push(rows2[i]);
                  }

                  result.ticket_2 = ticket_2;

                  cb(null, result);
                }
                else{
                  console.log('Error while performing Query 2.', err2);
                  cb(err2,null);
                }
              });
            }
            else{
              console.log('Error while performing Query 3.');
              cb(err1,null);
            }
          });
      }
    }
    else{
      console.log('Error while performing Query 4.');
      cb(err0,null);
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
        console.log(rows[0]);
        cb(null, rows[0]);
      }
      else{
        console.log('Error while performing Query.', err);
        cb(err,null);
      }
  });
}

exports.getEmployeeByEmail = function (email, cb) {
  connection.query('select * from employee where email = ?',[email], function (err, rows, fields) {
    if (!err){
        //console.log(rows[0]);
        cb(null, rows[0]);
      }
      else{
        console.log('Error while performing Query.', err);
        cb(err,null);
      }
  });
}


exports.getStatistics = function(employee, cb){
  connection.query('select uploaded_routes, uploaded_tickets, validated_tickets, fraudulent_tickets, no_shows from employee where idemployee = ?', [employee], function (err, rows, fields) {
    if (!err){
        cb(null, rows[0]);
      }
      else{
        console.log('Error while performing Query.');
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