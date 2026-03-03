"use strict";

function getCurrentLanguage() {
    var supportLanguages = ["en-US", "zh-CN"];
    var supportLanguageCodes =
        [
            ["en", "en-au", "en-ca", "en-gb", "en-ie", "en-in", "en-nz", "en-sg", "en-us", "en-za", "en-bz", "en-hk", "en-id", "en-jm", "en-kz", "en-mt", "en-my", "en-ph", "en-pk", "en-tt", "en-vn", "en-zw", "en-053", "en-021", "en-029", "en-011", "en-018", "en-014"],
            ["zh-hans", "zh-cn", "zh-hans-cn", "zh-sg", "zh-hans-sg"]
        ];
    var fallbackLanguage = "en-US";
    var languages = navigator.languages || [navigator.language || fallbackLanguage];
    for (var i = 0; i < languages.length; i++) {
        var lang = languages[i].toLowerCase();
        for (var j = 0; j < supportLanguages.length; j++) {
            if (supportLanguageCodes[j].some(function (x) { return x === lang; })) {
                return supportLanguages[j];
            }
        }
    }
    return fallbackLanguage;
}

function loadResource(language) {
    var url = "./strings/" + language + "/resources.rejson";
    if (typeof fetch === "undefined") {
        return new function () {
            var isCompleted, results, error;
            var request = typeof XDomainRequest === "undefined" ? new XMLHttpRequest() : new XDomainRequest();
            function onError(ex) {
                isCompleted = true;
                error = ex;
            }
            function invokeCallbacks(callbacks, value) {
                for (var i = 0; i < callbacks.length; i++) {
                    try { callbacks[i](value); }
                    catch (_) { }
                }
            }
            var onFulfilleds = [];
            var onRejecteds = [];
            this.then = function (onFulfilled, onRejected) {
                if (isCompleted) {
                    if (error) {
                        if (onRejected) {
                            onRejected(error);
                        }
                    }
                    else {
                        if (onFulfilled) {
                            onFulfilled(results);
                        }
                    }
                }
                else {
                    if (onFulfilled) {
                        onFulfilleds.push(onFulfilled);
                    }
                    if (onRejected) {
                        onRejecteds.push(onRejected);
                    }
                }
            }
            this.GetAwaiter = function () {
                return ((new (EnchantsOrder.JSIL.Common.Promise$b1.Of(System.Object))(this))).GetAwaiter();
            }
            request.open("GET", url, true);
            request.onload = function () {
                try {
                    isCompleted = true;
                    results = typeof JSON === "undefined" ? eval('(' + request.responseText + ')') : JSON.parse(request.responseText);
                    invokeCallbacks(onFulfilleds, window.strings = results);
                }
                catch (ex) {
                    error = ex;
                    invokeCallbacks(onRejecteds, error);
                }
            };
            request.onerror = onError;
            request.ontimeout = onError;
            request.send();
        }
    }
    else {
        return fetch(url)
            .then(function (x) { return x.json(); })
            .then(function (x) { return window.strings = x; });
    }
}

(function () {
    var lang = getCurrentLanguage();
    if (lang !== "en-US") {
        loadResource(lang).then(function () {
            WinJS.Resources.processAll();
            document.documentElement.lang = lang;
        });
    }
})();