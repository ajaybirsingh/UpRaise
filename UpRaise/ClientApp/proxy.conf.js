const PROXY_CONFIG = [
    {
        context: [
            "/api/",
            "/swagger/",
            //"/_configuration",
            ///"/.well-known",
            //"/Identity",
            //"/connect",
            //"/ApplyDatabaseMigrations",
        ],

        "target": "https://localhost:5001",
        "secure": false,
        "logLevel": "debug"
    }

]

module.exports = PROXY_CONFIG;

