const express = require('express')
const app = express()
const port = 3000
const http = require('http');
var cors = require('cors')

app.use(cors())

const options = {
  host: '127.0.0.1',
  port: 15301,
  path: '/api/ipgpsfinder/v1/locations/list',
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  }
}

app.get('/', (req, res) => {
  http.get(options, (resp) => {
  let data = '';

  resp.on('data', (chunk) => {
    data += chunk;
  });

  resp.on('end', () => {
    res.send(JSON.parse(data));
  });

}).on("error", (err) => {
  console.log("Error: " + err.message);
});
  
})

app.listen(port, () => {
  console.log(`Example app listening at http://localhost:${port}`)
})