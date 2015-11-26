import MySQLdb

def readshares(file, db):
	cursor = db.cursor()
	with open(file, "r") as lines:
	    next(lines)
	    for line in lines:
	        array = line.split("|")
	    	#print array
	    	
	    	# Prepare SQL query to INSERT a record into the database.
	        sql = """INSERT INTO share(symbol, name) VALUES (%s, %s)"""

	        try:
	            #print array[0] + array[1]
	            # Execute the SQL command
	            cursor.execute(sql, (array[0], array[1]))
	            # Commit your changes in the database
	            db.commit()
	            print 'success' + array[0]
	        
	        except MySQLdb.Error, e:
	            # Rollback in case there is any error
	            db.rollback()
	            print "MySQL Error: %s" % str(e)

    
    

db = MySQLdb.connect("localhost","root","admin","new_york_stock_exchange")

readshares("nasdaqlisted.txt", db)
readshares("otherlisted.txt", db)

# disconnect from server
db.close()



		

