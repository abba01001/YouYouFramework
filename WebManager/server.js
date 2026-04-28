const express = require('express');
const path = require('path');
const http = require('http');
const { Server } = require('socket.io');

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
    cors: { origin: "*" } // 允许跨域连接
});

const port = 3000;
app.use(express.static('public'));

// 挂载 io 到 app，方便路由调用
app.set('io', io);

const utilsRouter = require('./routes/utils');
app.use('/api/utils', utilsRouter);

app.get('/get-title', (req, res) => {
    res.json({ title: process.env.APP_TITLE || "MyFramework Manager" });
});

server.listen(port, () => {
    console.log(`[Success] Server running at http://localhost:${port}`);
});