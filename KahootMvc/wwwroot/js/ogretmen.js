  function changeTab(tab, event) {
            var tabs = document.querySelectorAll('.tab');
    tabs.forEach(function(t) {
        t.classList.remove('active');
            });
    event.target.classList.add('active');

    var content = document.getElementById('tab-content');

    if (tab === 'raporlar') {
        content.innerHTML = '<div class="empty-state"><h2>📊 Raporlar</h2><p>Yakında eklenecek...</p></div>';
            } else if (tab === 'ayarlar') {
        content.innerHTML = '<div class="empty-state"><h2>⚙️ Ayarlar</h2><p>Yakında eklenecek...</p></div>';
            } else {
        location.reload();
            }
        }

    document.getElementById('quiz-form').addEventListener('submit', function(e) {
        e.preventDefault();
    var title = document.getElementById('quiz-title').value;

    if (title) {
        alert('Quiz başarıyla oluşturuldu: ' + title);
    closeModal();
            }
        });

    document.getElementById('modal').addEventListener('click', function(e) {
            if (e.target === this) {
        closeModal();
            }
    });
document.getElementById('createQuizBtn').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "CreateQuiz.html";
});// Daha sonra değiştireceğim sadece şimdilik test amaçlı bunlar