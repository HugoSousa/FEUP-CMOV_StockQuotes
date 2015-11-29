/** user.js **/
var database = require('../modules/database.js');
var moment = require('moment');
var jwt = require('jwt-simple');
var fs = require('fs');

var User = function (data) {  
    this.data = data;
    //console.log(this.data);
}

User.prototype.data = {};
User.prototype.username = {}
User.prototype.password = {}


User.prototype.validate = function () {
    console.log(this.data);  
	if ('username' in this.data && 'password' in this.data) {
		this.name = this.data.name;
		this.username = this.data.username;
		this.password = this.data.password;

		return true;
	}
	return false;
}

User.prototype.changeName = function (name) {  
    this.data.name = name;
}

User.login = function (username, password, res, app) {
        database.getUserByUsername(username, function (err, user) {

            if (err  || typeof user == 'undefined') {
                return res.status(400).json({
                    error: 'User not found'
                });
            }

            if (!user || password != user.password) {
                return res.status(400).json({
                    error: "Wrong Credentials"
                });
            }

            var expires = moment().add(7, 'days').valueOf();
            var token = jwt.encode({
                iss: user.login,
                exp: expires
            }, app.get('jwtTokenSecret'));

            delete user.password;
            res.json({
                token  : token,
                expires: expires,
                user   : user
            });
        })
};

module.exports = User;