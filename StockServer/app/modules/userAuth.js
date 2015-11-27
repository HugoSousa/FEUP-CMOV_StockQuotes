var jwt = require('jwt-simple');
var database = require('../modules/database.js');

module.exports = function (req, res, next) {
        var token = (req.body && req.body.access_token) || (req.query && req.query.access_token) || req.headers['x-access-token'];

        if (token) {
            try {
                var decoded = jwt.decode(token, 'hastodosecrettosecrettootell');

                if (decoded.exp <= Date.now()) {
                        return res.status(403).json( { error: "Access token has expired" });
                } else {
                    var username = decoded.iss;

                    database.getUserByUsername(username, function (err, user) {
                        if (err || !user)
                            return res.status(403).json({ error: 'User not found' });
                        delete user.password; // line can be removed, only here to avoid sending password back where it should no longer be needed
                        req.user = user;
                        req.user.permission = 'user';
                        return next();
                    });


                }

            } catch (err) {
                return res.status(403).json( {error: "Error parsing token", info : err});
            }
        } else {
            return res.status(403).json( {error: "Missing token"});
        }
}


