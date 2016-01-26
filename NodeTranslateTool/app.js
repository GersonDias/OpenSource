#! /usr/bin/env node

var translator = require('bingtranslator');
var credentials = {
	clientId: 'NodeTranslator',
	clientSecret: '87HmRonzpIy/LwwB6mQUsmJIzLJpglN+vHihVZWcSTA='
};

var textToBeTranslated = process.argv[2];
var originLanguage = "en-US";
var destinatinationLanguage = "pt-BR";

var processArgs = function(){
	if (process.argv.length < 3 || process.argv.length > 5) {
		console.log("Usage: translate \"text\" [<source>] [<dest>]");
		return;
	}

	if (process.argv.length == 5)
	{
		originLanguage = process.argv[3];
		destinatinationLanguage = process.argv[4];
	}

	if (process.argv.length == 4)
	{
		originLanguage = "";
		destinatinationLanguage = process.argv[3];
	}

	if (originLanguage == "")
	{
		translator.detect(credentials, textToBeTranslated, function(err, from){
			if (catchErr(err)) return;

			originLanguage = from;
		})
	}
};

var catchErr = function(err)
{
	if (err){
		console.log('error', err);
		return true;
	}

	return false;
}

var main = function(){
	processArgs();
	
	translator.translate(credentials, textToBeTranslated, originLanguage, destinatinationLanguage, function(err, translated){
		if (catchErr(err)) return;
		console.log(translated);
	})
}

main();





