$(document).ready(function () {
    $("#loginBtn").click(function (e) {
        e.preventDefault();

        var email = $("#loginemail").val().trim();
        var password = $("#loginsifre").val().trim();

        // Validasyon
        if (email.length < 3) {
            alert("Email veya kullanýcý adý en az 3 karakter olmalýdýr.");
            return;
        }

        if (password.length < 4) {
            alert("Þifre en az 4 karakter olmalýdýr.");
            return;
        }

        var data = {
            Email: email,
            Password: password
        };

        $.ajax({
            url: '/GirisYap',
            type: 'POST',
            data: data,
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else {
                    alert(response.message);
                }
            },
            error: function (xhr) {
                alert("Bir hata oluþtu: " + xhr.responseText);
            }
        });
    });
});
