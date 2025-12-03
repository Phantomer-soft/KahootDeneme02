function showSignup() {
    document.querySelector(".form").classList.remove("active");
    document.querySelector(".form").classList.add("hidden");
    document.querySelector(".form-kaydol").classList.remove("hidden");
    document.querySelector(".form-kaydol").classList.add("active");
    document.querySelector(".form-kaydol-2").classList.remove("active");
    document.querySelector(".form-kaydol-2").classList.add("hidden");

    var welcome = document.getElementById("welcome");
    welcome.innerHTML = "Wahoot'a kaydol";
    welcome.classList.add("welcome-style");

    var text = document.getElementById("text");
    text.innerHTML = "Her adımda yeni bir soru, her köşede seni bekleyen bir rakip var.";
    text.classList.add("text-style");
}

function showLogin() {
    document.querySelector(".form-kaydol").classList.remove("active");
    document.querySelector(".form-kaydol").classList.add("hidden");
    document.querySelector(".form-kaydol-2").classList.remove("active");
    document.querySelector(".form-kaydol-2").classList.add("hidden");
    document.querySelector(".form").classList.remove("hidden");
    document.querySelector(".form").classList.add("active");

    var welcome = document.getElementById("welcome");
    welcome.innerHTML = "Wahoot'a hoş geldiniz.";
    welcome.classList.add("welcome-style");

    var text = document.getElementById("text");
    text.innerHTML = "Yarışmaya devam etmek için giriş yap, bilginle rakiplerini geride bırak!";
    text.classList.add("text-style");
}
function showSignup2() {
    document.querySelector(".form-kaydol-2").classList.remove("hidden");
    document.querySelector(".form-kaydol-2").classList.add("active");
    document.querySelector(".form").classList.remove("active");
    document.querySelector(".form").classList.add("hidden");
    document.querySelector(".form-kaydol").classList.remove("active");
    document.querySelector(".form-kaydol").classList.add("hidden");

}

// Sayfa yüklendiğinde giriş formunu göster
document.addEventListener('DOMContentLoaded', function () {
    document.querySelector(".form").classList.add("active");
    document.querySelector(".form-kaydol").classList.add("hidden");
    document.querySelector(".form-kaydol-2").classList.add("hidden");

    var welcome = document.getElementById("welcome");
    welcome.innerHTML = "Wahoot'a hoş geldiniz.";
    welcome.classList.add("welcome-style");

    var text = document.getElementById("text");
    text.innerHTML = "Yarışmaya devam etmek için giriş yap, bilginle rakiplerini geride bırak!";
    text.classList.add("text-style");


});


