let username = null;
let chatroom = null;
let firebaseConfig = null;
let db = null;
let fetchChat;
let messaging;
let senderToken = null;
let reciverToken = null;

function InitFirebase(firebaseConfig) {

    // Initialize Firebase
    firebase.initializeApp(firebaseConfig);

    // initialize database
    db = firebase.database();

    //Firebase Notification Permition.
    messaging = firebase.messaging();

    //Getting device permition and tocken
    messaging.requestPermission().then(function () {
        return messaging.getToken();
    }).then(function (token) {
        console.log(token);
        senderToken = token;
    }).catch(function (error) {
        console.log("Error Accord");
    });
}

// fetch data from databsae and set values in fetchChat
function fetchMessage(senderName) {

    const fetchChat = db.ref("ChatRoom/" + chatroom + "/messages/");

    fetchChat.on("child_added", function (snapshot, prevChildKey) {

        const messages = snapshot.val();

        const datetime = new Date(messages.timestamp);
        const time = datetime.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true })
        const gmtdate = datetime.toGMTString().split(" ");
        const date = gmtdate[2] + " " + gmtdate[1] + " " + gmtdate[3]
        let html = "";

        if (senderName === messages.sedername) {

            html += "<div class=\"outgoing_msg\">" +
                "<div class=\"sent_msg\">" +
                "<p>" + messages.message + "</p>" +
                "<span class=\"time_date\"> " + time + " | " + date + "</span>" +
                "</div>" +
                "</div>";
        }
        else {
            if (reciverToken == null) {
                reciverToken = messages.sendertoken;
            }

            var splitName = messages.sedername.split(" ");
            html += "<div class=\"incoming_msg\">" +
                "<div class=\"incoming_msg_img\">" +
                splitName[0].charAt(0) +
                splitName[1].charAt(0) +
                "</div>" +
                "<div class=\"received_msg\">" +
                "<div class=\"received_withd_msg\">" +
                "<p>" + messages.message + "</p>" +
                "<span class=\"time_date\"> " + time + " | " + date + "</span>" +
                "</div>" +
                "</div>" +
                "</div>";
        }

        $("#chat_history").append(html);
    });
}

function offconn() {
    const fetchChat = db.ref("ChatRoom/" + chatroom + "/messages/");
    fetchChat.off();
}

// send message to db and also create a collection it self if not in db
function sendMessage(model) {

    db.ref("ChatRoom/" + chatroom + "/messages/" + model.timestamp).set({
        sedername: model.sedername,
        sendertoken: senderToken,
        recivername: model.recivername,
        message: model.message,
        timestamp: model.timestamp,
        image: model.image,
    });

    sendNotification(reciverToken,model);
}

function sendNotification(token, msgModel) {

    var myHeaders = new Headers();
    myHeaders.append("Content-Type", "application/json");
    myHeaders.append("Authorization", "Bearer AAAAnLJ5aXo:APA91bGxpghiD_7FaqfYPDxtyXapWKnfd3vuz3rnUPu4IgkXPLSKcDktyLwETVVzi0Cm47uAQMsMM8GKMkKMVJTvrhnA101VOZgtCQWoSmOKw-ZD5cjYlJ4aFLp1gUaVJp6JrzyDBBc7");

    var raw = JSON.stringify({
        "notification": {
            "title": msgModel.sedername,
            "body": msgModel.message,
            "icon": "https://aephyweb.azurewebsites.net/assets/img/EmailLogo.png"
        },
        "to": token
    });

    var requestOptions = {
        method: 'POST',
        headers: myHeaders,
        body: raw,
        redirect: 'follow'
    };

    fetch("https://fcm.googleapis.com/fcm/send", requestOptions)
        .then(response => response.text())
        .then(result => console.log(result))
        .catch(error => console.log('error', error));
}
