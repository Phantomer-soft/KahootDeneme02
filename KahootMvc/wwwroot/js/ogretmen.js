function escapeHtml(text) {
    if (text === null || text === undefined) {
        return "";
    }
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}
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

async function loadQuizzes() {
    const quizList = document.getElementById('quiz-list');
    quizList.innerHTML = '<div class="loading">📚 Yükleniyor...</div>';

    try {
        const userId = localStorage.getItem("userid");

        if (!userId) {
            throw new Error("UserId localStorage'da yok");
}

        const response = await fetch(
            `/teacher/Quiz/GetQuizzes?userid=${localStorage.getItem("userid")}`,
            {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            }
        );

        if (!response.ok) {
            throw new Error("Veriler yüklenemedi");
        }

        const quizzes = await response.json();

        if (!Array.isArray(quizzes) || quizzes.length === 0) {
            quizList.innerHTML = `
                <div class="empty-state">
                    <h2>📚 Henüz quiz eklenmemiş</h2>
                    <p>Yeni bir quiz eklemek için yukarıdaki butona tıklayın</p>
                </div>
            `;
            return;
        }

        quizList.innerHTML = '';

        quizzes.forEach(q => {
            const quizId = q.quizId ?? q.QuizId;
            const title = q.title ?? q.Title;
            const description = q.description ?? q.Description;
            const questionCount = q.questionCount ?? q.QuestionCount;

            const quizCard = document.createElement('div');
            quizCard.className = 'quiz-card';
            quizCard.innerHTML = `
                <div class="quiz-info">
                    <h3>${escapeHtml(title)}</h3>
                    ${description ? `<p>${escapeHtml(description)}</p>` : ''}
                    <span class="badge">📝 ${questionCount} Soru</span>
                </div>
                <div class="quiz-actions">
                    <button class="btn btn-success" onclick="startQuiz('${quizId}')">▶️ Başlat</button>
                    <button class="btn btn-warning" onclick="editQuiz('${quizId}')">✏️ Düzenle</button>
                    <button class="btn btn-danger" onclick="deleteQuiz('${quizId}')">🗑️ Sil</button>
                </div>
            `;
            quizList.appendChild(quizCard);
        });

    } catch (error) {
        console.error(error);
        quizList.innerHTML = `
            <div class="empty-state">
                <h2>❌ Bir hata oluştu</h2>
                <p>${error.message}</p>
                <button class="btn btn-primary" onclick="loadQuizzes()">🔄 Tekrar Dene</button>
            </div>
        `;
    }
}
function startQuiz(quizId) {
    alert('Starting quiz ' + quizId);
    // Implement start quiz logic
}

function editQuiz(quizId) {
    window.location.href = `/edit.html`;
    localStorage.setItem("editQuiz",quizId)
}

async function deleteQuiz(quizId) {
    if (confirm('Bu quizi silmek istediğinizden emin misiniz?')) {
        try {
            const response = await fetch(`/teacher/quiz/delete/${quizId}`, {
                method: 'DELETE'
            });
            if (response.ok) {
                await loadQuizzes(); // Listeyi yeniden yükle
            } else {
                throw new Error('Quiz silinemedi');
            }
        }
        catch (error) {
            console.error('Hata:', error);
            alert(error.message);
        }
    }
}

// Sayfa yüklendiğinde quizleri yükle
document.addEventListener('DOMContentLoaded', loadQuizzes);