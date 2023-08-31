// See https://aka.ms/new-console-template for more information
// Publisher app: Will publish message to broker
// message will be produced from databaseadoptor

using Publisher;

var app = new Application(args);
app.DisplayInfo();
app.RegisterConfigs();
app.RegisterServices();
await app.RunAsync();

