﻿let TotalDaysCount = 0;
let CustomProjectDetialsId = 0;
$('#ProjectDetailsbtn').on('click', function () {
    var form = $("form[name='ProjectDetailsForm']");
    form.validate({
        rules: {
            customprojectoutline: { required: true },
            customprojectdescription: { required: true },
            //customProjectDuration: { required: true },
            /*customPrice: { required: true }*/
        },
        messages: {
            customprojectoutline: { required: "Please Project outline" },
            customprojectdescription: { required: "Please enter Project description" },
            //customProjectDuration: { required: "Please enter Project Duration" },
           /* customPrice: { required: "Please enter Price" }*/
        }
    });
    if (form.valid() === true) {
        //GetWorkingOurForm(this);
        SaveCustomSolutionData();
    }
});

$('#ProjectContinuebtn').on('click', function () {
    ResetCustomeForm();
    window.location.href = '/Home/Dashboard#activeProjects'
});

/*$('#WorkingFormbtn').on('click', function () {
    var form = $("form[name='CustomWorkingInfoForm']");
    form.validate({
        rules: {
            customstartDate: { required: true },
            customendDate: { required: true },
            CustomstartTime: { required: true },
            CustomendTime: { required: true }
        },
        messages: {
            customstartDate: { required: "Please select proper start date" },
            customendDate: { required: "Please select proper end date" },
            CustomstartTime: { required: "Please select proper start time" },
            CustomendTime: { required: "Please select proper End Date" }
        }
    });
    if (form.valid() === true) {
        SaveCustomSolutionData();
    }
});*/

$('#postProjectInfo').on('click', function () {
    var form = $("form[name='ProjectInfoForm']");
    form.validate({
        rules: {
            ProjectTitle: { required: true },
            ProjectDescription: { required: true },
            projFile: { required: true },
            DeliveryTime: { required: true },
            BudgetAmount: { required: true }
        },
        messages: {
            ProjectTitle: { required: "Please enter title" },
            ProjectDescription: { required: "Please enter description" },
            projFile: { required: "Please attach your project document" },
            DeliveryTime: { required: "Please select date to deliver project" },
            BudgetAmount: { required: "Please enter BudgetAmount" }
        }
    });
    if (form.valid() === true) {
        GetClientForm(this);
    }
});


$("form[name='landingPageSolutionSearchBox']").validate({
    rules: {
        //searchText: { required: true }
    },
    messages: {
        //searchText: { required: "Please insert solution name to search.." }
    },
    errorPlacement: function (error, element) {
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
        }
        else {
            error.insertAfter(element);
        }
    },
    submitHandler: function (form) {
        landingPageSearchFromSolutions();
    }
});


$('#delivery-time').on('change', function () {
    var deliveryTime = $(this).val();

    var d = new Date();

    var month = d.getMonth() + 1;
    var day = d.getDate();

    var currentDate = d.getFullYear() + '-' +
        (month < 10 ? '0' : '') + month + '-' +
        (day < 10 ? '0' : '') + day;
});

$('#postWorkingInfo').on('click', function () {
    var form = $("form[name='workingInfoForm']");
    form.validate({
        rules: {
            startDate: { required: true },
            endDate: { required: true },
            startTime: { required: true },
            endTime: { required: true }
        },
        messages: {
            startDate: { required: "Please select start date" },
            endDate: { required: "Please select end date" },
            startTime: { required: "Please select starting hours" },
            endTime: { required: "Please select ending hours" }
        }
    });
    if (form.valid() === true) {
        GetClientForm(this);
    }
});

function CloseprivacyPolicyModal() {
    $("#privacyPolicyModal").modal('hide')
}
function OpenprivacyPolicyModal() {
    $("#privacyPolicyModal").modal('show')
}

$('[data-search]').on('keyup', function () {
    var searchVal = $(this).val();
    var filterItems = $('[data-filter-item]');
    if (searchVal != '') {
        filterItems.parent().addClass('d-none');
        $('[data-filter-item][data-filter-name*="' + searchVal.toLowerCase() + '"]').parent().removeClass('d-none');
    } else {
        filterItems.parent().removeClass('d-none');
    }
});

$('[data-filter-item]').on('click', function () {
    var text = $(this).data('filterName');
    $('[data-search]').val(text);
});

$('#start-date').on('change', function () {
    getBusinessDays();
});
$('#end-date').on('change', function () {
    getBusinessDays();
});

function getBusinessDays() {
    var firstDate = $('#start-date').val();
    var lastDate = $('#end-date').val();
    var totalBusinessDays = 0;
    if (firstDate != 'undefined' && lastDate != 'undefined' && firstDate != '' && lastDate != '') {
        if (firstDate < lastDate) {
            var calcUrl = "/LandingPage/BusinessDaysUntil?firstDay=" + firstDate + "&lastDay=" + lastDate;
            $.ajax({
                type: "GET",
                url: calcUrl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (businessDays) {
                    totalBusinessDays = businessDays;
                    if (totalBusinessDays > 0) {
                        var label = "Total business Days : " + totalBusinessDays;
                        $('#labelWorkDays').text(label);
                        $('#labelWorkDays').css('display', 'block');
                        sessionStorage.setItem("BusinessDays", totalBusinessDays);
                    } else {
                        sessionStorage.setItem("BusinessDays", 0);
                    }
                },
                error: function (result) {
                    showToaster("error", "Error !", result.responseText);
                    $('#labelWorkDays').css('display', 'none');
                    sessionStorage.setItem("BusinessDays", 0);
                }
            });
        } else {
            showToaster("error", "Error !", "'End-Date' must be larger than 'Start-Date'..");
            $('#labelWorkDays').css('display', 'none');
            sessionStorage.setItem("BusinessDays", 0);
        }
    } else {
        $('#labelWorkDays').text('');
        //$('#labelWorkDays').css('display', 'none');
        sessionStorage.setItem("BusinessDays", 0);
    }
}

function SaveRequestedProposal() {
    $("#preloader").show();
    var fileUpload = $("#projFile").get(0);
    var files = fileUpload.files;

    var Solutiondata = {
        ID: 0,
        ServiceId: $('#drp-popServices').val(),
        SolutionId: $('#drp-popSolutions').val(),
        IndustryId: $('#drp-popIndustries').val(),
        SolutionTitle: $('#ProjectTitle').val(),
        SoultionDescription: $('#ProjectDescription').val(),
        DeliveryTime: $('#delivery-time').val(),
        Budget: $('#BudgetAmount').val(),
        StartDate: $('#start-date').val(),
        EndDate: $('#end-date').val()
    }
    var formData = new FormData();
    for (var i = 0; i < files.length; i++) {
        formData.append("httpPostedFileBase", files[i]);
    }
    formData.append("SolutionData", JSON.stringify(Solutiondata));
    $.ajax({
        type: "POST",
        url: "/LandingPage/SaveRequestedProposal",
        contentType: false,
        processData: false,
        dataType: "json",
        data: formData,
        success: function (result) {
            $("#preloader").hide();
            if (result != null && result.StatusCode != 200) {
                showToaster("error", "Error !", result.Message);
            } else {
                showToaster("success", "Success !", result.Message);
            }
        },
        error: function (result) {
            $("#preloader").hide();
            if (result.Message != null || result.Message != "") {
                showToaster("error", "Error !", result.Message);
            } else {
                showToaster("error", "Error !", result.responseText);
            }
        }
    });
}

function GetClientForm(data) {
    if (checkFromWhereClick == "SelectAFreelancer" || checkFromWhereClick == "SelectAFreelancerMenu"
        || checkFromWhereClick == "ReuestAProposal") {

        if (checkFromWhereClick == "SelectAFreelancerMenu") {
            $('#SelectFreelancerPreviousButton').css('display', 'block');
        } else {
            $('#SelectFreelancerPreviousButton').css('display', 'none');
        }
        var serviceValidator = $('#service-validate');
        var solutionValidator = $('#solution-validate');
        var industryalidator = $('#industry-validate');
        if ($("#drp-popServices").val() == "All" || $("#drp-popSolutions").val() == "All" || $("#drp-popIndustries").val() == "All") {
            if ($("#drp-popServices").val() == "All") {
                serviceValidator.text('This field is required.');
                serviceValidator.css('display', 'inline-block');
            } else {
                serviceValidator.text('');
                serviceValidator.css('display', 'none');
            }
            if ($("#drp-popSolutions").val() == "All") {
                solutionValidator.text('This field is required.');
                solutionValidator.css('display', 'inline-block');
            } else {
                solutionValidator.text('');
                solutionValidator.css('display', 'none');
            }

            if ($("#drp-popIndustries").val() == "All") {
                industryalidator.text('This field is required.');
                industryalidator.css('display', 'inline-block');
            } else {
                industryalidator.text('');
                industryalidator.css('display', 'none');
            }

        }
        else {
            industryalidator.css('display', 'none');
            solutionValidator.css('display', 'none');
            serviceValidator.css('display', 'none');


            $("#confirmation").modal('hide');
            $('#CustomiseProjectPopUp').modal('show');
            $("#FreelancerDetailsForm").css("display", "block");
            $("#customisedDetailsForm").css("display", "none");
            $("#customisedProjectWorkingForm").css("display", "none");
            if (checkFromWhereClick == "ReuestAProposal") {
                $("#multiple-freelancerselected").show();
                $("#single-freelancerSelected").hide();
            }
            else {
                $("#multiple-freelancerselected").hide();
                $("#single-freelancerSelected").show();
            }



            $("#BrowseSolutionPreviousButton").css("display", "none");

            $("#freelancer-title").html("Select Freelancer");
            var solutionid = $('#drp-popSolutions').val();
            var industryid = $('#drp-popIndustries').val();
            $("#custom-solutionId").val(solutionid);
            $("#custom-IndustryId").val(industryid);
            return;
        }
    }

    var Buttonclickvalue = $('#PostButton').text();
    if (data.text == "Confirm") {
        $("#confirmation").modal('hide')
        var businessDaysCount = sessionStorage.getItem("BusinessDays");
        if (businessDaysCount != null && businessDaysCount > 0) {
            $("#preloader").show();
            var data = {
                Id: 0,
                StartDate: $('#start-date').val(),
                EndDate: $('#end-date').val(),
                SolutionId: $('#drp-popSolutions').val(),
                IndustryId: $('#drp-popIndustries').val(),
                HolidaysList: $('#holidaysLst').val(),
                isExcludeWeekends: $('#excludeWeekends').is(':checked')
            }
            $.ajax({
                type: "POST",
                url: "/LandingPage/SaveClientAvailability",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(data),
                success: function (result) {
                    //showToaster("success", "Success !", result.Message);
                },
                error: function (result) {
                    showToaster("error", "Error !", "Something went wrong !!");
                }
            });
            SaveRequestedProposal();
        }
    }
    if (data.text == "Previous") {
        if ($("#ClientForm").is(":visible")) {
            $("#ServicesForm").css("display", "block")
            $("#ClientForm").css("display", "none")
            $("#PreviousButton").css("display", "none")
            $("#PostButton").text("Next")
            $("#WorkingHourForm").css("display", "none")
        }
        if ($("#WorkingHourForm").is(":visible")) {
            $("#ServicesForm").css("display", "none")
            $("#ClientForm").css("display", "block")
            $("#PostButton").text("Next")
            $("#PreviousButton").css("display", "block")
            $("#WorkingHourForm").css("display", "none")
        }
    }
    else {
        var serviceValidator = $('#service-validate');
        var solutionValidator = $('#solution-validate');
        var industryalidator = $('#industry-validate');
        if ($("#drp-popServices").val() == "All" || $("#drp-popSolutions").val() == "All" || $("#drp-popIndustries").val() == "All") {
            if ($("#drp-popServices").val() == "All") {
                serviceValidator.text('This field is required.');
                serviceValidator.css('display', 'inline-block');
            } else {
                serviceValidator.text('');
                serviceValidator.css('display', 'none');
            }
            if ($("#drp-popSolutions").val() == "All") {
                solutionValidator.text('This field is required.');
                solutionValidator.css('display', 'inline-block');
            } else {
                solutionValidator.text('');
                solutionValidator.css('display', 'none');
            }

            if ($("#drp-popIndustries").val() == "All") {
                industryalidator.text('This field is required.');
                industryalidator.css('display', 'inline-block');
            } else {
                industryalidator.text('');
                industryalidator.css('display', 'none');
            }

        }
        else {
            industryalidator.css('display', 'none');
            solutionValidator.css('display', 'none');
            serviceValidator.css('display', 'none');
            if ($("#ServicesForm").is(":visible")) {
                $("#ServicesForm").css("display", "none")
                $("#ClientForm").css("display", "block")
                $("#WorkingHourForm").css("display", "none")
                $("#PreviousButton").css("display", "block")
                $("#PostButton").text("Next")
                return
            }
            if ($("#ClientForm").is(":visible")) {
                $("#ServicesForm").css("display", "none")
                $("#ClientForm").css("display", "none")
                $("#WorkingHourForm").css("display", "block")
                $("#PreviousButton").css("display", "block")
                $("#PostButton").text("Confirm")
            }
        }
    }

}

$('#SelectFreelancerPreviousButton').on('click', function () {
    $('#CustomiseProjectPopUp').modal('hide');
    CustomizeProject('SelectAFreelancerMenu');
});

function ClosePostRequestModalPopup() {
    $("#confirmation").modal('hide')
}

function clearForm() {
    $("#ID").val("");
    $("#FreelancerID").val("");
    $("#ServiceID").val("");
    $("#IndustriesID").val("");
    $("#SolutionID").val("");
    $("#Title").val("");
    $("#Level").val("");
    $("#Description").val("");
    $("#CVPath").val("");
}

function OpenPostModalPopup(clickPlace) {
    checkFromWhereClick = clickPlace;
    sessionStorage.setItem("FromWhereClicked", clickPlace);
    if (checkFromWhereClick == "CustomizeProject") {
        OpenFreelancerPopUp();
        $('#SelectFreelancerPreviousButton').css('display', 'none');

    }
    else if (checkFromWhereClick == "SelectAFreelancer") {
        $('#SelectFreelancerPreviousButton').css('display', 'none');
        $('#CustomiseProjectPopUp').modal('show');
        $("#FreelancerDetailsForm").css("display", "block")
        $("#customisedDetailsForm").css("display", "none")
        $("#customisedProjectWorkingForm").css("display", "none")
        //$("#BrowseSolutionPostButton").text("Next")
        $("#BrowseSolutionPreviousButton").css("display", "none");
        $("#multiple-freelancerselected").hide();
        $("#single-freelancerSelected").show();
        $("#freelancer-title").html("Select Freelancer");
        var urlParams = new URLSearchParams(window.location.search);
        var solutionid = urlParams.get('Solution');
        var industryid = urlParams.get('Industry');
        $("#custom-solutionId").val(solutionid);
        $("#custom-IndustryId").val(industryid);
    }
    else {
        $('#service-validate').css('display', 'none');
        $('#solution-validate').css('display', 'none');
        $('#industry-validate').css('display', 'none');
        $("#drp-popIndustries").val("All")
        $("#drp-popServices").val("All")
        $("#drp-popSolutions").val("All")
        $("#confirmation").modal('show');
        $('#labelWorkDays').css('display', 'none');
        $("#start-hour").val("");
        $("#end-hour").val("");
        $("#end-date").val("");
        $("#start-date").val("");
        $("#delivery-time").val("");
        $('#holidaysLst').val("");
        $('#excludeWeekends').prop('checked');
        $("#ServicesForm").css("display", "block")
        $("#ClientForm").css("display", "none")
        $("#PreviousButton").css("display", "none")
        $("#WorkingHourForm").css("display", "none")
        $("#PostButton").text("Next")
        BindIndustries()
        BindServices()
        BindSolution()
    }
    
}

$('#ProjectDescriptionPreviousButton').on('click', function () {
    var fromWhereClicked = sessionStorage.getItem("FromWhereClicked");
    if (checkFromWhereClick == "CustomizeProject") {
        $('#CustomiseProjectPopUp').modal('hide');
        CustomizeProject('CustomizeProject')
    }
    else if (checkFromWhereClick == "SelectAFreelancer") {
        $('#CustomiseProjectPopUp').modal('hide');
        OpenPostModalPopup("SelectAFreelancer");
    }
    else {
        $('#CustomiseProjectPopUp').modal('hide');
        checkFromWhereClick == "SelectAFreelancerMenu";
        GetClientForm(this);
    }
});

function BindIndustries() {
    $.ajax({
        type: "POST",
        url: "/Home/GetIndustryList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {

            $("#drp-popIndustries").empty();
            $("#drp-popIndustries").append($("<option/>").val("All").text("All Industries"));
            $.each(result.Result, function (data, value) {
                $("#drp-popIndustries").append($("<option     />").val(value.Id).text(value.IndustryName));
            })

        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function BindServices() {
    $.ajax({
        type: "Get",
        url: "/Home/GetServicesList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {

            $("#drp-popServices").empty();
            $("#drp-popServices").append($("<option/>").val("All").text("All Services"));
            $.each(result.Result, function (data, value) {
                $("#drp-popServices").append($("<option     />").val(value.Id).text(value.ServicesName));
            })

        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function BindSolution() {
    $.ajax({
        type: "Get",
        url: "/Home/GetSolutionList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {

            $("#drp-popSolutions").empty();
            $("#drp-popSolutions").append($("<option/>").val("All").text("All Solutions"));
            $.each(result.Result, function (data, value) {
                $("#drp-popSolutions").append($("<option     />").val(value.Id).text(value.Title));
            })

        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function ServicesValue() {
    var ServiceId = $("#drp-popServices").val()
    if (ServiceId == "All") {
        ServiceId = 0
    }
    var data = {
        Id: ServiceId
    }
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetSolutionBasedonServices",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result.Result.length != 0) {
                var data = result.Result.SolutionData;
                $("#drp-popSolutions option:not(:first)").remove()
                $("#drp-popIndustries option:not(:first)").remove()
                $.each(data, function (data, value) {
                    $("#drp-popSolutions").append($("<option     />").val(value.Id).text(value.Title));
                })
                $.each(result.Result.IndustriesData, function (datas, values) {
                    $("#drp-popIndustries").append($("<option     />").val(values.Id).text(values.IndustryName));
                })

            }
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function SolutionValues() {
    var SolutionId = $("#drp-popSolutions").val()
    if (SolutionId == "All") {
        ServicesValue()
        return
    }
    var data = {
        Id: SolutionId
    }
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetSolutionBasedonSolutionSelected",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result.Result.length != 0) {
                var data = result.Result.SolutionData;
                $("#drp-popIndustries option:not(:first)").remove()
                $.each(result.Result.IndustriesData, function (datas, values) {
                    $("#drp-popIndustries").append($("<option     />").val(values.Id).text(values.IndustryName));
                })
            }
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function landingPageSearchFromSolutions() {
    searchData = $('[data-search]').val();
    if (searchData != '') {
        var compareData = '';
        compareData = $('[data-filter-item][data-filter-name*="' + searchData.toLowerCase() + '"]').data('solutionName');
        if (compareData != '') {
            window.location = "../LandingPage/BrowseSolutions?solution=" + searchData;
        }
    }
}

$('[data-search]').on('keyup', function () {
    var searchVal = $(this).val();
    var filterItems = $('[data-filter-item]');
    if (searchVal != '') {
        filterItems.parent().addClass('d-none');
        $('[data-filter-item][data-filter-name*="' + searchVal.toLowerCase() + '"]').parent().removeClass('d-none');
        $('#landingPageSearch-list').css('display', 'block');
    } else {
        $('#landingPageSearch-list').css('display', 'none');
        filterItems.parent().removeClass('d-none');
    }
});
function GetPopularSolutionLandingPage() {
    $.ajax({
        type: "Get",
        url: "/LandingPage/GetSolutionList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result != 0) {
                var data = result.Result
                //data = data.slice(0, 6);
                var popularAiSection = "";
                var searchList = "";
                $.each(data, function (data, value) {
                    var title = value.Title.length > 50 ? value.Title.substring(0, 50) + "..." : value.Title;

                    searchList += '<li class="dropdown-item">' +
                        '<div data-filter-item data-filter-name="' + title.toLowerCase() + '" class="search-item" onclick="landingPageSetToSearch(' + "'" + title + "'" + ');">' + title + '</div>' +
                        '</li>';
                })
                // $('#ai-Section').html(popularAiSection);
                $('.carousel-item').first().addClass('active');
                $('.carousel-indicators > li').first().addClass('active');

                $('#landingPageSearch-list').html(searchList);
                var items2 = document.querySelectorAll('.carousel .carousel-item')

                items2.forEach((el) => {
                    const minPerSlide = 4
                    let next = el.nextElementSibling
                    for (var i = 1; i < minPerSlide; i++) {
                        if (!next) {
                            next = items2[0]
                        }
                        let cloneChild = next.cloneNode(true)
                        el.appendChild(cloneChild.children[0])
                        next = next.nextElementSibling
                    }
                })
            }
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });

}

function landingPageSetToSearch(searchData) {
    var text = searchData;
    $('[data-search]').val(text);
    $('#landingPageSearch-list').css('display', 'none');
}

function OpenFreelancerPopUp() {
    $('#CustomiseProjectPopUp').modal('show');
    $("#multiple-freelancerselected").show();
    $("#single-freelancerSelected").hide();
    $("#FreelancerDetailsForm").css("display", "block")
    $("#customisedDetailsForm").css("display", "none")
    $("#customisedProjectWorkingForm").css("display", "none")
    $("#BrowseSolutionPostButton").text("Next")
    $("#BrowseSolutionPreviousButton").css("display", "none");
    $("#freelancer-title").html("Select Freelancers")

    var urlParams = new URLSearchParams(window.location.search);
    var solutionid = urlParams.get('Solution');
    var industryid = urlParams.get('Industry');

    $("#custom-solutionId").val(solutionid);
    $("#custom-IndustryId").val(industryid);


}

function CloseFreelancerPopUp() {
    $('#CustomiseProjectPopUp').modal('hide');
}

function GetWorkingOurForm(data) {
    // if (data.text == "Confirm") {
    //     // $("#CustomiseProjectPopUp").modal('hide')
    //     SaveCustomSolutionData();
    // }
    $('#CustomiseProjectPopUp').modal('show');
    //if (data.text == "Next") {
    if (true) {
        if ($("#FreelancerDetailsForm").is(":visible")) {
            GetMiletoneList();
            $("#FreelancerDetailsForm").css("display", "none")
            $("#customisedDetailsForm").css("display", "block")
            $("#customisedProjectWorkingForm").css("display", "none")
            $("#BrowseSolutionPostButton").text("Next")
            $("#BrowseSolutionPreviousButton").css("display", "block")
            return;
        }
        if ($("#customisedDetailsForm").is(":visible")) {
            $("#FreelancerDetailsForm").css("display", "none")
            $("#customisedDetailsForm").css("display", "none")
            $("#customisedProjectWorkingForm").css("display", "block")
            $("#BrowseSolutionPostButton").text("Confirm")
            $("#BrowseSolutionPreviousButton").css("display", "block")
            return;
        }
    }
    if (data.text == "Previous") {
        if ($("#customisedDetailsForm").is(":visible")) {
            $("#FreelancerDetailsForm").css("display", "block")
            $("#customisedDetailsForm").css("display", "none")
            $("#customisedProjectWorkingForm").css("display", "none")
            $("#BrowseSolutionPostButton").text("Next")
            $("#BrowseSolutionPreviousButton").css("display", "none")
            return;
        }
        if ($("#customisedProjectWorkingForm").is(":visible")) {
            $("#FreelancerDetailsForm").css("display", "none")
            $("#customisedDetailsForm").css("display", "block")
            $("#customisedProjectWorkingForm").css("display", "none")
            $("#BrowseSolutionPostButton").text("Next")
            $("#BrowseSolutionPreviousButton").css("display", "block")
            return;
        }

    }
}

function OpenMileStonePopup() {
    $("#MileStonePopModal").modal('show')
    GetMiletoneList();
}

function SaveMilStoneData() {
    var singleFreelancer = $('input[name="fav_freelancer"]:checked').val();
    var projectType = $('input[name="projSizeweb"]:checked').val();
    if (singleFreelancer != undefined) {
        projectType = "custom";
    }

    var MileStoneData = {
        Id: $("#MileStoneId").val(),
        Description: $("#mileStoneDescription").val(),
        Title: $("#mileStoneTitle").val(),
        //DueDate: $("#mileStonedueDate").val(),
        Days: $("#txtMilestoneDays").val(),
        SolutionId: $("#custom-solutionId").val(),
        IndustryId: $("#custom-IndustryId").val(),
        ProjectType: projectType,
        CustomProjectDetailId: CustomProjectDetialsId
    };
    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/SaveMileStone",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(MileStoneData),
        success: function (result) {
            $("#preloader").hide();
            showToaster("success", "Success", result.Message);
            ResetMileStoneForm()
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function ResetMileStoneForm() {
    $("#MileStoneId").val(0);
    $("#mileStoneDescription").val("");
    $("#mileStoneTitle").val("");
    $("#txtMilestoneDays").val("0");
    $("#mileStonedueDate").val("");
    //$("#MileStonePopModal").modal('hide')
    GetMiletoneList()
}

function CloseMileStoneForm() {
    $("#MileStoneId").val(0);
    $("#mileStoneDescription").val("");
    $("#mileStoneTitle").val("");
    $("#txtMilestoneDays").val("0");
    $("#mileStonedueDate").val("");
    $("#MileStonePopModal").modal('hide');
}

function GetMiletoneList() {

    var singleFreelancer = $('input[name="fav_freelancer"]:checked').val();
    var projectType = $('input[name="projSizeweb"]:checked').val();
    if (singleFreelancer != undefined) {
        projectType = "custom";
    }

    var data = {
        SolutionId: $("#custom-solutionId").val(),
        IndustryId: $("#custom-IndustryId").val(),
        ProjectType: projectType,
        CustomProjectDetailId: CustomProjectDetialsId
    }

    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetMiletoneList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            console.log(result);
            TotalDaysCount = 0;
            var data = result.Result.MileStoneList
            var index = 0;
            var subObj = '';
            var htm = '';
            for (index = 0; index < data.length; index++) {
                subObj = data[index];
                console.log(subObj)
                htm += '<tr>';
                //htm += '<td class="" >' + subObj.Id + '</td>';
                htm += '<td onclick=EditMiletoneData(' + subObj.Id + ') class=cls-editnum>' + subObj.Id + '</td>';
                htm += '<td>' + subObj.Description + '</td>';
                htm += '<td>' + subObj.Title + '</td>';
                htm += '<td>' + subObj.Days + '</td>';
                htm += '<td><a class="btn btn-danger btn-sm" onclick=DeleteMileStoneById(' + subObj.Id + ')>Delete</a></td>';
                htm += '</tr>';

                if (subObj.Days != null && subObj.Days != 0) {
                    TotalDaysCount += subObj.Days;
                }
            }
            $("#MileStoneTable").find("tr:gt(0)").remove();
            $("#MileStoneTable tbody").append(htm);
            $("#customProjectDuration").val(TotalDaysCount);
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
            $("#preloader").hide();
        }
    });
}

function DeleteMileStoneById(Id) {
    Swal.fire({
        title: 'Are you sure you want to delete milestone?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then(function (t) {
        if (!t.isConfirmed) return;
        var Data = {
            Id: Id,
        };
        $.ajax({
            type: "POST",
            url: "/Home/DeleteMileStone",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(Data),
            success: function (result) {
                showToaster("success", "Success", result.Message);
                GetMiletoneList()
            },
            error: function (result) {
                showToaster("error", "Error !", result.responseText);
            }
        });

    });
}

function EditMiletoneData(id) {
    var data = {
        Id: id,
    };
    $.ajax({
        type: "POST",
        url: "/Home/GetMileStoneById",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result != null) {
                $("#MileStonePopModal").modal('show')
                var data = result.Result;
                $("#MileStoneId").val(data.Id);
                $("#mileStoneDescription").val(data.Description);
                $("#txtMilestoneDays").val(data.Days);
                $("#mileStoneTitle").val(data.Title);
                $("#mileStonedueDate").val(moment(data.DueDate).format('YYYY-MM-DD'));
            }
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function OpenPointsPopup() {
    GetPointsList();
    $("#PointsPopModal").modal('show')

}

function SavePointsData() {

    var pointId = $("#PointsId").val();
    if (pointId == 0) {
        if ($("#PointsTable tbody tr").length >= 3) {
            showToaster("warning", "Warning !", "you have reached a limit for adding points");
            return;
        }
    }


    var singleFreelancer = $('input[name="fav_freelancer"]:checked').val();
    var projectType = $('input[name="projSizeweb"]:checked').val();
    if (singleFreelancer != undefined) {
        projectType = "custom";
    }

    var PointsData = {
        Id: pointId,
        PointKey: $("#pointsKey-id").val(),
        PointValue: $("#pointsValue-id").val(),
        SolutionId: $("#custom-solutionId").val(),
        IndustryId: $("#custom-IndustryId").val(),
        ProjectType: projectType,
        CustomProjectDetialsId: CustomProjectDetialsId
    };

    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/SavePoints",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(PointsData),
        success: function (result) {
            $("#preloader").hide();
            showToaster("success", "Success", result.Message);
            ResetPointsForm()
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function ResetPointsForm() {
    $("#PointsId").val(0);
    $("#pointsKey-id").val("");
    $("#pointsValue-id").val("");
    // $("#PointsPopModal").modal('hide')
    GetPointsList()
}

function ClosePointsForm() {
    ResetPointsForm();
    $("#PointsPopModal").modal('hide')
}

function GetPointsList() {

    var singleFreelancer = $('input[name="fav_freelancer"]:checked').val();
    var projectType = $('input[name="projSizeweb"]:checked').val();
    if (singleFreelancer != undefined) {
        projectType = "custom";
    }

    var data = {
        SolutionId: $("#custom-solutionId").val(),
        IndustryId: $("#custom-IndustryId").val(),
        ProjectType: projectType,
        CustomProjectDetailId: CustomProjectDetialsId
    }

    $("#preloader").show();
    $.ajax({
        type: "POST",
        url: "/Home/GetPointsList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            var data = result.Result
            var index = 0;
            var subObj = '';
            var htm = '';
            for (index = 0; index < data.length; index++) {
                subObj = data[index];
                if (subObj.PointKey == null || subObj.PointKey == undefined) {
                    subObj.PointKey = ""
                }
                if (subObj.PointValue == null || subObj.PointValue == undefined) {
                    subObj.PointValue = ""
                }
                htm += '<tr>';
                htm += '<td class=cls-editnum onclick=GetPointsDataById(' + subObj.Id + ')>' + subObj.Id + '</td>';
                htm += '<td>' + subObj.PointKey + '</td>';
                htm += '<td>' + subObj.PointValue + '</td>';
                htm += '<td><a class="btn btn-danger btn-sm" onclick=DeletePointsById(' + subObj.Id + ')>Delete</a></td>';
                htm += '</tr>';
            }
            $("#PointsTable").find("tr:gt(0)").remove();
            $("#PointsTable tbody").append(htm);
            $("#preloader").hide();
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
            $("#preloader").hide();
        }
    });
}

function GetPointsDataById(id) {
    var data = {
        Id: id,
    };
    $.ajax({
        type: "POST",
        url: "/Home/GetPointsDataById",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result != null) {
                $("#PointsPopModal").modal('show')
                var data = result.Result;
                $("#PointsId").val(data.Id);
                $("#pointsKey-id").val(data.PointKey);
                $("#pointsValue-id").val(data.PointValue);
            }
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function DeletePointsById(Id) {

    Swal.fire({
        title: 'Are you sure you want to delete points?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then(function (t) {
        if (!t.isConfirmed) return;

        var Data = {
            Id: Id,
        };
        $.ajax({
            type: "POST",
            url: "/Home/DeletePoints",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(Data),
            success: function (result) {
                showToaster("success", "Success", result.Message);
                GetPointsList()
            },
            error: function (result) {
                showToaster("error", "Error !", result.responseText);
            }
        });
    });
}

function SaveCustomSolutionData() {

    var singleFreelancer = $('input[name="fav_freelancer"]:checked').val();
    var Issinglefreelancer = false;
    var projectType = $('input[name="projSizeweb"]:checked').val();
    if (singleFreelancer != undefined) {
        Issinglefreelancer = true;
        projectType = "custom";
    }

    var data = {
        Id: 0,
        TotalAssociate: $("#total-associate").val(),
        TotalExpert: $("#total-expert").val(),
        TotalProjectManager: $("#total-projectmanager").val(),
        CustomProjectOutline: $("#custom-ProjectOutline").val(),
        CustomProjectDetail: $("#custom-ProjectDescription").val(),
        CustomProjectDuration: '0',
        CustomPrice: $("#custom-price").val(),
        //CustomStartDate: $("#Customstart-date").val(),
        //CustomEndDate: $("#Customend-date").val(),
        CustomStartDate: '',
        CustomEndDate: '',
        //CustomExcludeWeekend: $('#CustomexcludeWeekends').is(':checked'),
        CustomExcludeWeekend: false,
        CustomOtherHolidayList: $("#CustomholidaysLst").val(),
        CustomStartHour: '',
        CustomEndHour: '',
        SolutionId: parseInt($("#custom-solutionId").val()),
        IndustryId: parseInt($("#custom-IndustryId").val()),
        ProjectType: projectType,
        IsSingleFreelancer: Issinglefreelancer,
        SingleFreelancer: singleFreelancer
    }

    $("#preloader").show();

    $.ajax({
        type: "POST",
        url: "/Home/SaveCustomSolutionData",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (result) {
            if (result.Message == "Project Initated Successfully!") {
                //ResetCustomeForm();
                //window.location.href = '/Home/Dashboard#activeProjects'
                $("#ProjectContinuebtn").removeClass("disabled");
                $("#AddHighlightsbtn").removeClass("disabled");
                $("#AddMileStonebtn").removeClass("disabled");
                $("#ProjectDetailsbtn").addClass("disabled");
                CustomProjectDetialsId = result.Result;
            } else {
                showToaster("error", "Error !", result.Message);
            }
            $("#preloader").hide();
        },
        error: function (result) {
            $("#preloader").hide();
            ResetCustomeForm();
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function ResetCustomeForm() {
    $("#total-associate").val("");
    $("#total-expert").val("");
    $("#total-projectmanager").val("");
    $("#custom-ProjectOutline").val("");
    $("#custom-ProjectDescription").val("");
    $("#Customproject-Duartion").val("");
    $("#custom-price").val(0);
    $("#Customstart-date").val("");
    $("#Customend-date").val("");
    $('#CustomexcludeWeekends').prop('checked', false);;
    $("#CustomholidaysLst").val("");
    $("#customstart-hour").val("");
    $("#customend-hour").val("");
    $("#custom-solutionId").val("");
    $("#custom-IndustryId").val("");
    $("#CustomiseProjectPopUp").modal('hide')
    $("#ProjectContinuebtn").addClass("disabled");
    $("#AddHighlightsbtn").addClass("disabled");
    $("#AddMileStonebtn").addClass("disabled");
}

function CloseCustomisePopUp() {
    $('input[name="preferOption"][value="select"]').prop('checked', 'checked');
    $('input[name="preferOptionmobile"][value="select"]').prop('checked', 'checked');
    $('#btn-ProjectInitiateOrSubmit').data('isInitiate', true);
    $('#CustomiseProjectPopUp').modal('hide');

    $("#custom-ProjectOutline").val('');
    $("#custom-ProjectDescription").val('');
    $("#custom-price").val(0);
}

let excludeData = [];

$("#btnAddDate").click(function () {

    if ($("#exclude-time").val() == '') {
        showToaster("error", "Required !", "Please select date !!");
    }
    else {

        $.ajax({
            type: "POST",
            url: "/Home/SaveFreelancerExcludeDate",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ DateRange: $("#exclude-time").val() }),
            dataType: "json",
            success: function (result) {
                if (result.StatusCode == 200) {
                    showToaster("success", "Saved.", "Data has been saved.");
                    bindFreelancerExcludeGrid();
                    $("#exclude-time").val('');
                }
                else {
                    $("#preloader").hide();
                    showToaster("error", "Error !", result.Message);
                }
            },
            error: function (result) {
                $("#preloader").hide();
                showToaster("error", "Error !", result.responseText);
            }
        });
    }
});

function bindFreelancerExcludeGrid() {
    $.ajax({
        type: "GET",
        url: "/Home/GetFreelancerExcludeDateData",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            //console.log(result.Result);
            if (result.StatusCode == 200) {
                $('#table-excludeDate').DataTable({
                    destroy: true,
                    searching: false,
                    paging: false,
                    info: false,
                    data: result.Result,
                    "columns": [
                        {
                            "data": "ExcludeDate",
                            "render": function (data, type, full, meta) {
                                var date = moment(data).format('DD-MM-YYYY');;
                                return '<span>' + date + '</span>';
                            }
                        },
                        {
                            "data": "action",
                            "render": function (data, type, row) {
                                return "<div class=\"link_button\" onclick=\"removeFreelancerExclude(" + row.Id + ")\">Delete</div>";
                            }
                        }
                    ]
                });
            }
            $("#preloader").hide();
            $("#table-excludeDate").removeAttr("style");
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
            $("#preloader").hide();
        }
    });
}

function removeFreelancerExclude(id) {
    $.ajax({
        type: "POST",
        url: "/Home/RemoveFreelancerExcludeDate",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ id: id }),
        dataType: "json",
        success: function (result) {
            showToaster("success", "Removed", "Data has been deleted.");
            bindFreelancerExcludeGrid();
            $("#exclude-time").val('');
        },
        error: function (result) {
            $("#preloader").hide();
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function OpenClientWorkingHoursPopUp(data) {
    if (data == "UpdateData") {
        $('#btn-ProjectInitiateOrSubmit').data('isInitiate', false);
    } else {
        $('#btn-ProjectInitiateOrSubmit').data('isInitiate', true);
    }
    $("#preloader").show();
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            url: "/LandingPage/GetClientWorkingHours",
            success: function (result) {
                if (result.Message == "success") {
                    $("#ClientWorkingePopModal").modal('show');
                    $("#StartHours").val(moment(result.Result.StartHour).format('HH:mm'));
                    $("#EndHours").val(moment(result.Result.EndHour).format('HH:mm'));
                    $('#onMonday').prop('checked', result.Result.onMonday);
                    $('#onTuesday').prop('checked', result.Result.onTuesday);
                    $('#onWednesday').prop('checked', result.Result.onWednesday);
                    $('#onThursday').prop('checked', result.Result.onThursday);
                    $('#onFriday').prop('checked', result.Result.onFriday);
                    $('#onSaturday').prop('checked', result.Result.onSaturday);
                    $('#onSunday').prop('checked', result.Result.onSunday);
                } else {
                    showToaster("error", "Error !", result.Message);
                }

                $("#preloader").hide();
            },
            error: function (result) {
                $("#preloader").hide();
                showToaster("error", "Error !", result.responseText);
            }
        });
        bindFreelancerExcludeGrid();
        $('input[name="ExcludeTime"]').daterangepicker({
            autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        });

        $('input[name="ExcludeTime"]').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
        });

        $('input[name="ExcludeTime"]').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
        });   
}

function InitiateOrUpdate() {
    var isInitiate = $('#btn-ProjectInitiateOrSubmit').data('isInitiate');
    if (isInitiate == true) {
        ProjectInitiated(false);
    } else {
        var calendarData = {
            Id: "",
            StartDate: $('#startDate').val(),
            EndDate: $('#endDate').val(),
            StartHour: $('#StartHours').val(),
            EndHour: $('#EndHours').val(),
            IsWeekendExclude: $('#excludeWeekends').is(':checked'),
            IsNotAvailableForNextSixMonth: $('#unavailableNext6Months').is(':checked'),
            IsWorkEarlier: $('#IsWorkEarlier').is(':checked'),
            IsWorkLater: $('#IsWorkLater').is(':checked'),
            StartHoursEarlier: $('#StartHoursEarlier').val(),
            EndHoursEarlier: $('#EndHoursEarlier').val(),
            StartHoursLater: $('#StartHoursLater').val(),
            EndHoursLater: $('#EndHoursLater').val(),
            onMonday: $('#onMonday').is(':checked'),
            onTuesday: $('#onTuesday').is(':checked'),
            onWednesday: $('#onWednesday').is(':checked'),
            onThursday: $('#onThursday').is(':checked'),
            onFriday: $('#onFriday').is(':checked'),
            onSaturday: $('#onSaturday').is(':checked'),
            onSunday: $('#onSunday').is(':checked')
        };

        var formData = new FormData();
        formData.append("CalendarData", JSON.stringify(calendarData));

        $('#preloader').show();
        $.ajax({
            type: "POST",
            url: "/Home/SaveCalenderData",
            contentType: false,
            processData: false,
            data: formData,
            success: function (result) {
                var test = result.split(",")[1].split(":")[1]
                showToaster("success", "Success", test.replace(/\"/g, ""));
                //GetUserData();
                CloseClientWorkingHourForm();
                GetWorkingOurForm(); 
                //OpenFreelancerPopUp();
                $('#preloader').hide();
            },
            error: function (result) {
                $('#preloader').hide();
                showToaster("error", "Error !", result.responseText);
            }
        });
    }
}

function CloseClientWorkingHourForm() {
    $('#btn-ProjectInitiateOrSubmit').data('isInitiate', true);
    $('input[name="preferOption"][value="select"]').prop('checked', 'checked');
    $('input[name="preferOptionmobile"][value="select"]').prop('checked', 'checked');
    $("#ClientWorkingePopModal").modal('hide');
}

function GetNotificationDetails() {
    $.ajax({
        type: "Get",
        url: "/Home/GetAllUnReadNotification",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Result != 0) {
                var data = result.Result;
                if (data != "") {
                    var totalLength = data.length;
                    var notificationSection = ""
                    $.each(data, function (datas, values) {
                        var timeAgo = moment(values.NotificationTime).fromNow();

                        notificationSection += '<li class="notification-item">' +
                            '<i class="bi bi-info-circle text-primary" > </i>' +
                            '<div>' +
                            '<h4>' + values.NotificationTitle + ' </h4>' +
                            '<p> ' + values.NotificationText + ' </p>' +
                            '<p> ' + timeAgo + ' </p>' +
                            '</div>' +
                            '</li>' +
                            '<li>' +
                            '<hr class="dropdown-divider" >' +
                            '</li>';

                    })
                    notificationSection += '<li class="dropdown-footer">' +
                       /* '<a> Show all notifications </a>' +*/
                        '</li>';
                    $(notificationSection).appendTo('#notification-section')
                    $("#total-notification").html(totalLength);
                    var msg = " You have " + totalLength + " new notifications"
                    $("#totalnotification-msg").html(msg);
                    //var msg = $("#totalnotification-msg").text().trim();
                    //msg.replace("{{ total_notification }}",totalLength)

                }

            } else {
                var msg = " You have 0 new notifications"
                $("#totalnotification-msg").html(msg);
                //var msg = $("#totalnotification-msg").text().trim();
                //msg.replace("{{ total_notification }}", 0)
            }
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}

function SignOut() {
    $.ajax({
        type: "Get",
        url: "/Home/LogOut",
        success: function (result) {
            window.location.href = '/Home/Login'
        },
        error: function (result) {
            showToaster("error", "Error !", result.responseText);
        }
    });
}