const express = require('express');
const path = require('path');
const app = express();
const port = 3000;

// 1. 基础配置
const app_title = process.env.APP_TITLE || "MyFramework Manager";
app.use(express.static('public'));

// 2. 挂载模块化路由
const utilsRouter = require('./routes/utils');
app.use('/api/utils', utilsRouter); // 所有工具类接口都以 /api/utils 开头

// 3. 基础接口
app.get('/get-title', (req, res) => {
    res.json({ title: app_title });
});

app.listen(port, () => {
    console.log(`${app_title} Server running at http://localhost:${port}`);
});