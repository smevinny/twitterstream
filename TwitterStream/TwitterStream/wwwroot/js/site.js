// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function startStreaming() {
    $.get("/api/TwitterStreaming/Start");
    $("#btn-start").hide();
    $("#btn-stop").show();
    window.setInterval(function () {
        getData();
    }, 1000);
}

function stopStreaming() {
    $.get("/api/TwitterStreaming/Stop", function (data, status) {
        window.location.reload();
    });
}

function getData() {
    $.getJSON('./data/twitter-analytics.json', function (res) {
        var keys = Object.keys(res);
        for (var key in keys) {
            var objKey = keys[key];
            if (objKey.indexOf("Top") > -1) {
                var topList = "";
                for (var i = 0; i < res[objKey].length; i++) {
                    var keyVal = res[objKey][i];
                    if (objKey.indexOf("tag") > -1) {
                        keyVal = "#" + keyVal
                    }
                    else if (objKey.indexOf("moji") > -1) {
                        keyVal = escape(keyVal);
                    }
                    debugger;
                    //decodeURIComponent(escape(s))
                    topList += "<span>" + keyVal + "</span><br/>";
                }
                $("#" + objKey).html(topList);
            }
            else {
                $("#" + objKey).html(res[objKey]);
            }
        }
    });
}

function startEmitter() {
    var emitter = emitter.connect({
        secure: true
    });
    var key = 'jtdQO-hb5jfujowvIKvSF41NeQOE8IoF';
    var vue = new Vue({
        el: '#app',
        data: {
            messages: []
        }
    });

    emitter.on('connect', function () {
        // once we're connected, subscribe to the 'tweet-stream' channel
        console.log('emitter: connected');
        emitter.subscribe({
            key: key,
            channel: "tweet-stream"
        });
    })

    // on every message, print it out
    emitter.on('message', function (msg) {
        alert(1);
        // log that we've received a message
        msg = msg.asObject();

        // make sure we load avatars from HTTPs scheme
        msg.avatar = msg.avatar.replace(/^http:\/\//i, 'https://');

        // If we have already 5 messages, remove the oldest one (first)
        if (vue.$data.messages.length >= 7) {
            vue.$data.messages.shift();
        }

        // Push the message we've received and update an identicon once it's there
        vue.$data.messages.push(msg);
    });
}
