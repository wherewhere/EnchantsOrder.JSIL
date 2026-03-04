/** @typedef {import("@types/winjs")} */
"use strict";

function getCurrentLanguage() {
    var supportLanguages = ["en-US", "zh-CN"];
    var supportLanguageCodes =
        [
            ["en", "en-au", "en-ca", "en-gb", "en-ie", "en-in", "en-nz", "en-sg", "en-us", "en-za", "en-bz", "en-hk", "en-id", "en-jm", "en-kz", "en-mt", "en-my", "en-ph", "en-pk", "en-tt", "en-vn", "en-zw", "en-053", "en-021", "en-029", "en-011", "en-018", "en-014"],
            ["zh-hans", "zh-cn", "zh-hans-cn", "zh-sg", "zh-hans-sg"]
        ];
    var fallbackLanguage = "en-US";
    var languages = navigator.languages || [navigator.language || navigator.userLanguage || navigator.browserLanguage || fallbackLanguage];
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

/**
 * @param {string} language
 */
function loadResource(language) {
    var url = "./strings/" + language + "/resources.rejson";
    if (typeof fetch === "undefined") {
        var IPromise = typeof Promise === "undefined" ? WinJS.Promise : Promise;
        return new IPromise(function (resolve, reject) {
            var request = new XMLHttpRequest();
            request.open("GET", url, true);
            request.onload = function () {
                try {
                    resolve(window.strings = JSON.parse(request.responseText));
                }
                catch (ex) {
                    reject(ex);
                }
            };
            request.onerror = reject;
            request.ontimeout = reject;
            request.send();
        });
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
        loadResource(lang).then(function () { WinJS.Resources.processAll(document.documentElement); });
    }
})();