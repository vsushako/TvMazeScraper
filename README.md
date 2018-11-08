## Assignment 
1. scrapes the TVMaze API for show and cast information; 
2. persists the data in storage; 
3. provides the scraped data using a REST API. 

## Business requirements: 
1. It should provide a paginated list of all tv shows containing the id of the TV show and a list of all the cast that are playing in that TV show. 
2. The list of the cast must be ordered by birthday descending. 

The REST API should provide a JSON response when a call to a HTTP endpoint is made 

## Components
TvMazeScraper.Api - main api service 
TvMazeScraper.IoC - IOC container to encapsulate realization methods from api
TvMazeScraper.Repository - repository pattern. To store data uses mongoDB
TvMazeScraper.Scheduler - scheduler to scrape data from http://www.tvmaze.com/api
TvMazeScraper.Service - main service what provides business logic
TvMazeScraper.Source - service what call TvMaze Api and gets data from it

### In MongoDB must be enabled transactions

Docker run example:
docker pull mongo
docker run -p 27017:27017 --name tvmazeapi -d mongo --smallfiles --replSet 'rs1' --storageEngine wiredTiger --port 27017
docker exec -i -t tvmazeapi mongo --port 27017 --eval "rs.initiate()"

MongoDb backup stored in TvMazeShows.7z file
