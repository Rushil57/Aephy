document.addEventListener('DOMContentLoaded', () => {
    "use strict";
    /**
    * Preloader
    */
    const preloader = document.querySelector('#preloader');
    if (preloader) {
        window.addEventListener('load', () => {
            $('#preloader').css("display", "none");
        });
    }
});

$(function () {
    $.validator.setDefaults({ ignore: ":hidden:not(.chosen-select)" })
    $("form[name='addSolutionForm']").validate({
        rules: {
            SolutionTitle: { required: true },
            Solution_SubTitle: { required: true },
            SolutionDescription: { required: true },
            SolutionServices: { required: true },
            Industries: { required: true }
            //SolutionImage: { required: true }
        },
        messages: {
            SolutionTitle: { required: "please enter solution title" },
            Solution_SubTitle: { required: "please enter sub title" },
            SolutionDescription: { required: "please enter description" },
            SolutionServices: { required: "please select valid services" },
            Industries: { required: "please select valid industries" }
            //SolutionImage: { required: "Please chhose an image" }
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
            addSolution()
        }
    });
    $(".chosen-select").chosen();
    BindServices()
    BindIndustries()

    //OpenGigRoles
    $("form[name='addRolesForm']").validate({
        rules: {
            drpSolutions : { required: true },
            Title: { required: true },
            drpLevel: { required: true },
        },
        messages: {
            drpSolutions: { required: "please select solution" },
            Title: { required: "please enter title" },
            drpLevel: { required: "please select level" },
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
            addRoles()
        }
    });
});

function addSolution() {
    var fileUpload = $("#SolutionImage").get(0);
    var files = fileUpload.files;

    var SolutionData = {
        Id: $("#SolutionId").val(),
        Title: $("#SolutionTitle").val(),
        SubTitle: $("#Solution_SubTitle").val(),
        Description: $("#SolutionDescription").val(),
        solutionServices: $("#drp-services").val(),
        solutionIndustries: $("#drp-industries").val(),
        ImagePath: $("#ImagePath").val()
    };

    var formData = new FormData();
    for (var i = 0; i < files.length; i++) {
        // fileData.append(files[i].name, files[i]);
        formData.append("httpPostedFileBase", files[i]);
    }
    formData.append("SolutionData", JSON.stringify(SolutionData));
    $("#preloader").show();
    $.ajax({
        url: '/Solutions/AddorEditSolution',
        type: "POST",
        contentType: false,
        processData: false,
        data: formData,
        dataType: 'json',
        success: function (result) {
            $("#preloader").hide();
            alert(result.Message);
            ResetForm();
            GetSolutionList();
        },
        error: function (err) {
            $("#preloader").hide();
            alert("Something Went Wrong!");
        }
    });

}

function SignOut() {
    $.ajax({
        type: "Get",
        url: "/Home/LogOut",
        success: function (result) {
            ClearHistory();
            window.location.href = '/Home/Login';
        },
        error: function (result) {
            alert("Something Went Wrong!");
        }
    });


function ResetForm() {
    $("#solution-img").remove()
    $("#SolutionTitle").val("");
    $("#Solution_SubTitle").val("");
    $("#SolutionDescription").val("");
    $("#SolutionImage").val("");
    $('#drp-services').val("").trigger('chosen:updated');
    $("#drp-industries").val("").trigger('chosen:updated');
    $("#addSolutionModal").modal('hide'),
        $("#SolutionId").val(0)
    $(".SolutionImage").attr('src', ""),
        $("#solutionImagepreview").hide(),
        $("#ImagePath").val("")
    $('#drp-industries option').attr('disabled', false);
    $('#drp-industries').trigger('chosen:updated');
}

$("#SolutionImage").change(AddsolutionImage);
function AddsolutionImage(event) {
    var input = this;
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = (function (e) {
            $("#solutionImagepreview").find("span").first().remove();
            $('#solutionImagepreview').show();
            var span = document.createElement('span');
            span.innerHTML =
                ['<img class="SolutionImage" src="', e.target.result, '" title="', escape(e.name), '" style="height:200px;width:200px;"/>']
                    .join('');
            document.getElementById('solutionImagepreview').insertBefore(span, null);
        });

        reader.readAsDataURL(input.files[0]);
    }
}



function ClearHistory() {
    var backlen = history.length;
    history.go(-backlen);
    window.location.href = '/Home/Dashboard';
}

function BindServices() {
    $.ajax({
        type: "Get",
        url: "/Lookup/GetServices",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            $.each(result.Result, function (data, value) {
                $("#drp-services").append($("<option     />").val(value.Id).text(value.ServicesName));
            })
            $('#drp-services').trigger('chosen:updated');
        },
        error: function (result) {
            alert("failure");
        }
    });
}

function BindIndustries() {
    $.ajax({
        type: "Get",
        url: "/Lookup/GetIndustries",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            $.each(result.Result, function (data, value) {
                $("#drp-industries").append($("<option     />").val(value.Id).text(value.IndustryName));
            })
            $('#drp-industries').trigger('chosen:updated');
        },
        error: function (result) {
            alert("failure");
        }
    });
}