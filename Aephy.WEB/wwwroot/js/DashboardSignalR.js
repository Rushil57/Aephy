"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/dashboardHub").build();

$(function () {
	connection.start().then(function () {
		/*alert('Connected to dashboardHub');*/
		invokeopenroles();
		invokeapprovedlist();

	}).catch(function (err) {
		return console.error(err.toString());
	});
});

// OpenGigRoles List
function InvokeOpenRoles() {
	getOpenRoles();
	/*connection.invoke("SendProducts").catch(function (err) {
		return console.error(err.toString());
	});*/
}

connection.on("ChnagedOpenRoles", function (statusBool) {
	/*BindProductsToGrid(products);*/
	if (statusBool) {
		getOpenRoles();
	}
});

//Approved List
function InvokeApprovedList() {
	GetApprovedList();
}

connection.on("ChangedApprovedList", function (statusBool) {
	if (statusBool) {
		GetApprovedList();
	}
});