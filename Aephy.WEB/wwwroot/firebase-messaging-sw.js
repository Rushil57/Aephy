'use strict';

importScripts('https://www.gstatic.com/firebasejs/8.10.1/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/8.10.1/firebase-messaging.js');

var firebaseConfig = {
    apiKey: "AIzaSyDmSAqmroEt3ZKQ5Z_noUwAU_lrdgNuUQA",
    authDomain: "demomessage-a0bc2.firebaseapp.com",
    databaseURL: "https://demomessage-a0bc2-default-rtdb.firebaseio.com",
    projectId: "demomessage-a0bc2",
    storageBucket: "demomessage-a0bc2.appspot.com",
    messagingSenderId: "673009199482",
    appId: "1:673009199482:web:77436aab135f0049180ec3",
    measurementId: "G-PWT7X1QHNL"
};

firebase.initializeApp(firebaseConfig);

const messaging = firebase.messaging();

//messaging.onBackgroundMessage((payload) => {
    
//    // Customize notification here
//    const notificationTitle = payload.notification.title;
//    const notificationOptions = {
//        body: payload.notification.body,
//        icon: 'https://aephyweb.azurewebsites.net/assets/img/EmailLogo.png'
//    };

//    self.registration.showNotification(notificationTitle, notificationOptions);
//});

//importScripts("https://www.gstatic.com/firebasejs/8.2.4/firebase-app.js");
//importScripts("https://www.gstatic.com/firebasejs/8.2.4/firebase-messaging.js");

//const FIREBASE_CONFIG = {
//	apiKey: "AIzaSyDmSAqmroEt3ZKQ5Z_noUwAU_lrdgNuUQA",
//	authDomain: "demomessage-a0bc2.firebaseapp.com",
//	databaseURL: "https://demomessage-a0bc2-default-rtdb.firebaseio.com",
//	projectId: "demomessage-a0bc2",
//	storageBucket: "demomessage-a0bc2.appspot.com",
//	messagingSenderId: "673009199482",
//	appId: "1:673009199482:web:77436aab135f0049180ec3",
//	measurementId: "G-PWT7X1QHNL"
//};

//// Initialize the firebase in the service worker.
//firebase.initializeApp(FIREBASE_CONFIG);

//self.addEventListener('push', function (event) {

//	debugger;
//	var data = event.data.json();

//	console.log(data);

//	const title = data.notification.title;

//	const options = {
//		body: data.notification.body
//	};
//	event.waitUntil(self.registration.showNotification(title, options));
//});

//self.addEventListener('notificationclick', function (event) { });

//self.addEventListener('notificationclose', function (event) { });