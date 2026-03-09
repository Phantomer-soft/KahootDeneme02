const quizApp = {
    connection: null,
    currentScore: 0,
    selectedAnswer: null,
    timerInterval: null,
    participants: [],

    init: function() {
        this.initSignalR();
    },

    // Bekleme overlay'ini göster
    showWaitingOverlay: function(type, message) {
        const overlay = document.getElementById('waitingOverlay');
        const content = document.getElementById('waitingContent');

        if (type === 'waiting') {
            content.innerHTML = `
            <h3>⏳ Bekliyoruz</h3>
            <div class="mini-loader"></div>
            <p>${message || 'Diğer öğrenciler cevaplıyor...'}</p>
            `;
        } else if (type === 'success') {
            content.innerHTML = `
            <h3>✅ Başarılı</h3>
            <div class="success-icon">🎉</div>
            <p>${message || 'Cevabınız alındı!'}</p>
            `;
        } else if (type === 'loading') {
            content.innerHTML = `
            <h3>📊 Sonuçlar</h3>
            <div class="mini-loader"></div>
            <p>${message || 'Liderlik tablosu hazırlanıyor...'}</p>
            `;
        }

        overlay.classList.remove('hidden');
    },

    // Bekleme overlay'ini gizle
    hideWaitingOverlay: function() {
        document.getElementById('waitingOverlay').classList.add('hidden');
    },

    initSignalR: async function() {
        this.connection = new signalR.HubConnectionBuilder()
        .withUrl("/QuizHub")
        .withAutomaticReconnect()
        .build();

        this.connection.on("SessionStarted", (message) => {
            document.getElementById('waitingSection').classList.add('hidden');
            document.getElementById('gameSection').classList.remove('hidden');
            this.showStatus('🚀 Oyun başladı!', 'success');
        });

        this.connection.on("SetSessionUser", (userId, sessionId) => {
            this.setSessionUser(userId, sessionId);
        });

        this.connection.on("EndSession", (leaderboard) => {
            this.hideWaitingOverlay();
            this.showStudentFinalResults(leaderboard);
        });

        this.connection.on("TimeUp", () => {
            this.hideWaitingOverlay();
            this.disableAnswers();
            this.showStatus('⏰ Süre doldu!', 'error');
            this.stopTimer();
            this.showWaitingOverlay('loading', 'Süre doldu! Sonuçlar hesaplanıyor...');
        });

        this.connection.on("UpdateLeaderboard", (leaderboard) => {
            this.hideWaitingOverlay();
            this.displayLeaderboard(leaderboard);
        });

        this.connection.on("SendQuestion", (data) => {
            this.hideWaitingOverlay();
            document.getElementById('leaderboardSection').classList.add('hidden');
            document.getElementById('questionContainer').classList.remove('hidden');
            this.displayQuestion(data);
            this.startTimer(data.time);
        });

        this.connection.on("UserJoined", (data) => {
            const joinedUser = data.username || data.Username;
            if (joinedUser) {
                this.participants.push(joinedUser);
                this.showStatus(`✨ ${joinedUser} katıldı!`, 'success');
            }
        });

        try {
            await this.connection.start();
            this.showStatus("Bağlantı Başarılı",'success')
        } catch (err) {
            this.showStatus("Bağlantı hatası ",'error');
        }
    },

    joinSession: async function() {
        const pinCode = document.getElementById('pinCode').value;
        const username = document.getElementById('username').value;

        if (!pinCode || !username) {
            this.showStatus('Lütfen tüm alanları doldurun!', 'error');
            return;
        }

        if (pinCode.length !== 6) {
            this.showStatus('PIN kodu 6 haneli olmalı!', 'error');
            return;
        }

        try {
            const response = await this.connection.invoke("Join", parseInt(pinCode), username);
            document.getElementById('loginSection').classList.add('hidden');
            document.getElementById('waitingSection').classList.remove('hidden');
            this.showStatus(`🎉 Hoş geldin ${response.username}!`, 'success');
        } catch (err) {
            this.showStatus('Hata: ' + err, 'error');
        }
    },

    setSessionUser: function(userId, sessionId) {
        localStorage.setItem("userId", userId);
        localStorage.setItem("sessionId", sessionId);
    },

    displayQuestion: function(data) {
        this.selectedAnswer = null;
        const container = document.getElementById('questionContainer');

        const html = `
        <div class="question-header">
        <div style="font-size: 1.2rem; color: #94a3b8;">Soru ${data.order}</div>
        <div class="timer" id="timer">${data.time}</div>
        </div>

        <div class="question-text">${data.text}</div>

        <div class="options">
        ${data.answers.map((opt, idx) => `
            <button class="option-btn" onclick="quizApp.selectAnswer('${opt.text}', this)">
            <strong>${String.fromCharCode(65 + idx)}</strong>
            ${opt.text}
            </button>
            `).join('')}
            </div>

            <button id="submitBtn" onclick="quizApp.submitAnswer()" disabled>📤 Cevapla</button>
            `;

            container.innerHTML = html;
    },

    selectAnswer: function(answer, button) {
        document.querySelectorAll('.option-btn').forEach(btn => {
            btn.classList.remove('selected');
        });

        button.classList.add('selected');
        this.selectedAnswer = answer;
        document.getElementById('submitBtn').disabled = false;
    },

    submitAnswer: async function() {
        if (!this.selectedAnswer) return;

        try {
            this.showWaitingOverlay('waiting', 'Cevabınız gönderiliyor...');

            await this.connection.invoke("SubmitAnswer", localStorage.getItem("sessionId"), this.selectedAnswer);

            setTimeout(() => {
                this.showWaitingOverlay('waiting', 'Diğer öğrenciler bekleniyor...');
            }, 500);

            this.disableAnswers();
            document.getElementById('submitBtn').disabled = true;
        } catch (err) {
            this.hideWaitingOverlay();
            this.showStatus('Hata: ' + err, 'error');
        }
    },

    startTimer: function(seconds) {
        let timeLeft = seconds;
        const timerEl = document.getElementById('timer');

        if (this.timerInterval) {
            clearInterval(this.timerInterval);
        }

        this.timerInterval = setInterval(() => {
            timeLeft--;
            timerEl.textContent = timeLeft;

            if (timeLeft <= 5) {
                timerEl.classList.add('warning');
            }

            if (timeLeft <= 0) {
                this.stopTimer();
            }
        }, 1000);
    },

    stopTimer: function() {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
    },

    disableAnswers: function() {
        document.querySelectorAll('.option-btn').forEach(btn => {
            btn.disabled = true;
        });
    },

    displayLeaderboard: function(leaderboard) {
        const container = document.getElementById('questionContainer');
        const leaderboardDiv = document.getElementById('leaderboardSection');
        const username = document.getElementById('username').value;

        container.classList.add('hidden');
        leaderboardDiv.classList.remove('hidden');

        let html = `
        <div class="card">
        <h2>🏆 Liderlik Tablosu</h2>
        <div class="leaderboard-container">
        `;

        leaderboard.forEach((user, index) => {
            const rankEmoji = index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : '';
            const rankNumber = rankEmoji ? '' : `${index + 1}.`;
            const isHighlight = user.userName === username ? 'highlight' : '';

            html += `
            <div class="leaderboard-item ${isHighlight}">
            <div class="rank">
            <span class="rank-emoji">${rankEmoji}</span>
            <span class="rank-number">${rankNumber}</span>
            </div>
            <span class="player-name">${user.userName}</span>
            <span class="player-score">${user.score}</span>
            </div>
            `;
        });

        html += `
        </div>
        <p class="leaderboard-footer">
        ⏳ Sonraki soru bekleniyor...
        </p>
        </div>
        `;

        leaderboardDiv.innerHTML = html;
    },

    showStudentFinalResults: function(leaderboard) {
        const container = document.getElementById('questionContainer');
        const leaderboardSection = document.getElementById('leaderboardSection');
        const gameSection = document.getElementById('gameSection');
        const myUsername = document.getElementById('username').value;

        container.classList.remove('hidden');
        leaderboardSection.classList.add('hidden');
        gameSection.classList.remove('hidden');

        const myRank = leaderboard.findIndex(u => u.userName === myUsername) + 1;
        const myScore = leaderboard.find(u => u.userName === myUsername)?.score || 0;

        const html = `
        <div class="game-over" style="text-align: center; padding: 20px;">
        <h2 style="font-size: 3rem; margin-bottom: 30px; background: linear-gradient(135deg, #60a5fa, #a78bfa); -webkit-background-clip: text; -webkit-text-fill-color: transparent;">
        🎯 Oyun Tamamlandı!
        </h2>

        <div class="score-display" style="background: linear-gradient(135deg, #1e293b, #0f172a); border: 1px solid rgba(255,255,255,0.1); border-radius: 32px; padding: 40px; margin: 30px 0;">
        <div style="font-size: 1.2rem; color: #94a3b8; margin-bottom: 10px;">Toplam Puanın</div>
        <div style="font-size: 6rem; font-weight: 800; background: linear-gradient(135deg, #60a5fa, #a78bfa); -webkit-background-clip: text; -webkit-text-fill-color: transparent; line-height: 1; margin: 20px 0;">
        ${myScore}
        </div>
        <div style="font-size: 1.5rem; color: #60a5fa; margin-top: 10px;">
        ${myRank}. sırada tamamladın!
        </div>
        </div>

        ${myRank === 1 ? `
            <div style="text-align: center; margin: 30px 0;">
            <div class="champion">🏆</div>
            <h3 style="color: #fbbf24; font-size: 2rem; margin: 20px 0;">ŞAMPİYON!</h3>
            </div>
            ` : ''}

            <div style="display: flex; justify-content: center; margin-top: 40px;">
            <button onclick="location.reload()" style="max-width: 300px; padding: 16px 32px; background: linear-gradient(135deg, #60a5fa, #a78bfa); border: none; border-radius: 16px; color: white; font-size: 1.1rem; font-weight: 600; cursor: pointer; transition: all 0.3s ease;">
            🔄 Yeni Oyuna Katıl
            </button>
            </div>
            </div>
            `;

            container.innerHTML = html;
    },

    showStatus: function(message, type) {
    const statusDiv = document.getElementById('status');
    statusDiv.className = `status ${type}`;
    statusDiv.textContent = message;
    
    setTimeout(() => {
        statusDiv.textContent = '';
        statusDiv.className = 'status'; 
    }, 5000); 
}
};

// Uygulamayı başlat
document.addEventListener('DOMContentLoaded', () => {
    quizApp.init();
});
