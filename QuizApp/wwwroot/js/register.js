$(document).ready(function () {
    $("#registerBtn").click(function (e) {
        e.preventDefault();

        var name = $("#ad").val().trim();
        var surname = $("#soyad").val().trim();
        var username = $("#kullaniciAdi").val().trim();
        var email = $("#e-mail").val().trim();
        var password = $("#sifre").val().trim();
        var passwordRepeat = $("#sifre-tekrar").val().trim();

        // Validasyon
        if (name.length < 2) { alert("Ad en az 2 karakter olmalıdır."); return; }
        if (surname.length < 2) { alert("Soyad en az 2 karakter olmalıdır."); return; }
        if (username.length < 3) { alert("Kullanıcı adı en az 3 karakter olmalıdır."); return; }
        if (email.length < 5 || email.indexOf("@") === -1) { alert("Geçerli bir email adresi girin."); return; }
        if (password.length < 6) { alert("Şifre en az 6 karakter olmalıdır."); return; }
        if (password !== passwordRepeat) { alert("Şifreler eşleşmiyor."); return; }

        var data = {
            Name: name,
            Surname: surname,
            Username: username,
            Email: email,
            Password: password
        };

        $.ajax({
            url: '/KayitOl',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    alert("Kayıt başarılı, giriş yapabilirsiniz.");
                    showLogin();
                    $("#ad, #soyad, #kullaniciAdi, #e-mail, #sifre, #sifre-tekrar").val("");
                } else {
                    alert("Kayıt başarısız: " + response.message);
                }
            },
            error: function (xhr) {
                alert("Bir hata oluştu: " + xhr.responseText);
            }
        });
    });
});
