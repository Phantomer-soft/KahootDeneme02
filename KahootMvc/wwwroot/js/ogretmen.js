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
        localStorage.removeItem("editQuiz");

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
                    <button class="btn btn-success" onclick="localStorage.setItem('selectedQuizId', '${quizId}'); window.location.href='createSession.html'">▶️ Başlat</button>
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
async function initSignalR() {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("https://localhost:7117/QuizHub")
                .withAutomaticReconnect()
                .build();

            // Event Listeners
            connection.on("UserJoined", (data) => {
                showStatus(`${data.Username} katıldı!`, 'success');
                participants.push(data.Username);
                updateParticipantsList(data.TotalUsers);
            });

            connection.on("SessionStarted", (message) => {
                showStatus(message, 'success');
                document.getElementById('waitingSection').classList.add('hidden');
                document.getElementById('gameSection').classList.remove('hidden');
            });

            connection.on("NewQuestion", (data) => {
                displayQuestion(data);
            });

            connection.on("ShowResults", (data) => {
                displayResults(data);
            });

            connection.on("SessionEnded", (data) => {
                showFinalResults(data);
            });

            try {
                await connection.start();
                console.log("SignalR Connected");
            } catch (err) {
                console.error(err);
                showStatus("Bağlantı hatası: " + err, 'error');
            }
        }
//odayı olustur 
async function createSession(quizId) {
            initSignalR();
            const teacherId = localStorage.getItem("teacherId");
            try {
                const response = await connection.invoke("CreateSession", teacherId, quizId);

                if(response.status)
                    alert("session olusturuldu");
                showStatus('Session başarıyla oluşturuldu!', 'success');
            } catch (err) {
                showStatus('Hata: ' + err, 'error');
            }
        }
// Oyunu Başlat
        async function startSession() {
            if (participants.length === 0) {
                showStatus('En az 1 katılımcı gerekli!', 'error');
                return;
            }

            try {
                await connection.invoke("StartSession", currentPinCode);
            } catch (err) {
                showStatus('Hata: ' + err, 'error');
            }
        }


function editQuiz(quizId) {
    window.location.href = `/edit.html`;
    localStorage.setItem("editQuiz",quizId)
}

async function deleteQuiz(quizId) {
    if (confirm('Bu quizi silmek istediğinizden emin misiniz?')) {
        try {
            const userId = localStorage.getItem('userid'); 
            
            if (!userId) {
                alert('Kullanıcı bilgisi bulunamadı. Lütfen tekrar giriş yapın.');
                return;
            }
            
            // quizId, URL sonuna query string olarak eklendi (?quizId=...)
            const response = await fetch(`/teacher/quiz/DeleteQuiz?quizId=${quizId}`, {
                method: 'DELETE', // GET yerine DELETE kullanıldı
                headers: {
                    'Content-Type': 'application/json',
                    'userId': userId // userId Header'da kalabilir
                }
            });
            
            if (response.ok) {
                alert('Quiz başarıyla silindi!');
                await loadQuizzes();
            } else {
                // Backend'den JSON dönmeyebilir (sadece Ok() veya Unauthorized()), bu yüzden kontrol ekledik
                const errorMessage = response.status === 401 ? 'Yetkiniz yok' : 'Quiz silinemedi';
                throw new Error(errorMessage);
            }
        }
        catch (error) {
            console.error('Hata:', error);
            alert(error.message);
        }
    }
}


// Sonraki Soru
        async function nextQuestion() {
            try {
                await connection.invoke("NextQuestion", currentPinCode);
            } catch (err) {
                showStatus('Hata: ' + err, 'error');
            }
        }

        // Soru Göster
        function displayQuestion(data) {
            const questionHtml = `
                <div style="margin-bottom: 15px;">
                    <span style="background: #667eea; color: white; padding: 8px 15px; border-radius: 20px;">
                        Soru ${data.QuestionNumber}/${data.TotalQuestions}
                    </span>
                    <span style="float: right; background: #f5576c; color: white; padding: 8px 15px; border-radius: 20px;">
                        ⏱️ ${data.TimeLimit} saniye
                    </span>
                </div>
                <div class="question-text">${data.QuestionText}</div>
                ${data.ImageUrl ? `<img src="${data.ImageUrl}" style="max-width: 100%; border-radius: 8px; margin: 15px 0;">` : ''}
                <div class="options">
                    ${data.Options.map((opt, idx) => `
                        <div class="option">${String.fromCharCode(65 + idx)}: ${opt}</div>
                    `).join('')}
                </div>
            `;
            document.getElementById('currentQuestionDisplay').innerHTML = questionHtml;
        }

        // Sonuçları Göster
        function displayResults(data) {
            const questionDiv = document.getElementById('currentQuestionDisplay');
            const options = questionDiv.querySelectorAll('.option');

            options.forEach(opt => {
                if (opt.textContent.includes(data.CorrectAnswer)) {
                    opt.classList.add('correct');
                }
            });

            if (data.Explanation) {
                questionDiv.innerHTML += `
                    <div style="margin-top: 20px; padding: 15px; background: #d4edda; border-radius: 8px;">
                        <strong>Açıklama:</strong> ${data.Explanation}
                    </div>
                `;
            }

            updateLeaderboard(data.Leaderboard);
        }

        // Sıralamayı Güncelle
        function updateLeaderboard(leaderboard) {
            const html = leaderboard.map((user, index) => `
                <div class="leaderboard-item">
                    <div>
                        <span style="font-size: 24px; margin-right: 10px;">
                            ${index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : index + 1}
                        </span>
                        <strong>${user.Username}</strong>
                    </div>
                    <div style="font-size: 24px; font-weight: bold; color: #667eea;">
                        ${user.Score} puan
                    </div>
                </div>
            `).join('');
            document.getElementById('liveLeaderboard').innerHTML = html;
        }

        // Final Sonuçları
        function showFinalResults(data) {
            document.getElementById('gameSection').classList.add('hidden');
            document.getElementById('resultsSection').classList.remove('hidden');

            const html = data.FinalLeaderboard.map((user, index) => `
                <div class="leaderboard-item">
                    <div>
                        <span style="font-size: 32px; margin-right: 10px;">
                            ${index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : index + 1}
                        </span>
                        <strong style="font-size: 20px;">${user.Username}</strong>
                    </div>
                    <div style="font-size: 28px; font-weight: bold; color: #667eea;">
                        ${user.Score} puan
                    </div>
                </div>
            `).join('');
            document.getElementById('finalLeaderboard').innerHTML = html;

            if (data.Winner) {
                showStatus(`🎉 Kazanan: ${data.Winner.Username} - ${data.Winner.Score} puan!`, 'success');
            }
        }

        // Katılımcı Listesi Güncelle
        function updateParticipantsList(count) {
            document.getElementById('participantCount').textContent = count;
            const html = participants.map(p => `
                <div class="participant">
                    <div style="font-size: 40px;">👤</div>
                    <div style="margin-top: 8px; font-weight: 600;">${p}</div>
                </div>
            `).join('');
            document.getElementById('participantsList').innerHTML = html;
        }

        // Durum Mesajı
        function showStatus(message, type) {
            const statusDiv = document.getElementById('status');
            statusDiv.className = `status ${type}`;
            statusDiv.textContent = message;
            setTimeout(() => statusDiv.textContent = '', 5000);
        }

// Sayfa yüklendiğinde quizleri yükle
document.addEventListener('DOMContentLoaded', loadQuizzes);
