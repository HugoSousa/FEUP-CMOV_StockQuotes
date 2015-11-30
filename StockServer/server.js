// BASE SETUP
// =============================================================================

// call the packages we need
var express    = require('express');
var bodyParser = require('body-parser');
var app        = express();
var morgan     = require('morgan');
var auth = require('./app/modules/userAuth.js');
var async = require('async');
var http = require('http');
var moment = require('moment');

// configure app
app.use(morgan('dev')); // log requests to the console

// configure body parser
app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

var port     = process.env.PORT || 8080; // set our port

var User     = require('./app/models/user');
var database = require('./app/modules/database.js');

//to change secret also change in jwtAuth
app.set('jwtTokenSecret', 'hastodosecrettosecrettootell');


// GENERAL ROUTING
// =============================================================================

var router = express.Router();

router.use(function(req, res, next) {
	console.log('Request arrived.');
	next();
});

router.get('/', function(req, res) {
	res.json({ result: {message:'Welcome to Stock Exchange API!' } });	
});

// USER ROUTING
// =============================================================================

router.route('/register')
	.post(function(req, res) {
		
		var new_user = new User(req.body);
		if (new_user.validate()) {
			database.registeruser(new_user, function (err, result) {

				if (err || result == null){ 
					console.log(err);
					res.status(400).json({result: {error: 'Duplicate username' }})
				}
				else
					res.json({ result: {message: 'Sucess'} })
			})
		}
		else {
			res.json({ result: {error:'Invalid user' } });
		}
		
	})

router.route('/login')
	.post(function(req, res) {
		if (req.body.username != undefined && req.body.username != "" && req.body.password != undefined && req.body.password != "") {
			User.login(req.body.username, req.body.password, res, req.app);
		} else {
			res.status(400).json({
				error: "Invalid request"
			});
		}
})

app.get("/api/testlogin", [auth], function (req, res) {
	res.send(req.user);
});

router.route('/portfolio')
	.get([auth], function(req, res) {
		var userid = req.user.iduser;
		
		database.getPortfolio(userid, function(err, data){
			if (err) {
	           res.status(400).json({error: err});              
	        } else {            
	            res.status(200).json(data);   
	        }
		});
});

router.route('/portfolio/add')
	.post([auth], function(req, res) {
		var userid = req.user.iduser;
		if (req.body.symbol != undefined && req.body.symbol != "") {
		database.addToPortfolio(userid, req.body.symbol, function(err, result){
					if (err) {
			           res.status(400).json({error: err});              
			        } else {            
			            res.status(200).json(result);   
			        }
				});
		}  else {            
	            res.status(200).json({error: "Missing share name"});   
	        }
		
});


router.get('/shares', function(req, res) {
	database.getAllShares(function(err, result) {
		if (err) res.status(401).json({error: err});
		else res.status(200).json(result);
	})
});

router.route('/share/:symbol')
	.get([auth], function(req, res) {

	var userid = req.user.iduser;
	var symbol = req.params.symbol;

	database.getShare(userid, symbol, function(err, result) {
		if (err) res.status(401).json({error: err});
		else res.status(200).json(result);
	})
});

router.get('/share/evolution/:symbol/:start?/:end?', function(req, res) {

	var symbol = req.params.symbol;
	var start;
	var end;

	if(req.params.start == null){
		start = moment().subtract(30, 'days'); 
	}else{
		start = moment(start); //yyyy-mm-dd
	}

	if(req.params.end == null){
		end = moment();
	}else{
		end = moment(end);
	}
	
	console.log(symbol);
	console.log(start);
	console.log(end);

	database.getShareEvolution(symbol, start, end, function(err, result) {
		if (err) res.status(401).json({error: err});
		else res.status(200).json(result);
	})
});	

app.use('/api', router);

// START THE SERVER
// =============================================================================
app.listen(port);
console.log('Listening on port ' + port);
