let tentativas = 0;

// ======================
// Função de erro estilo DOS
// ======================
function erroDOS() {
    tentativas++;

    const logs = [
        "ERROR 401 - ACCESS DENIED",
        "VERIFYING CREDENTIALS...",
        "USER NOT AUTHORIZED",
        "SECURITY FLAG RAISED",
        "CONTACT SYSTEM ADMINISTRATOR"
    ];

    const logBox = document.getElementById("dos-log");
    if (!logBox) return;

    logBox.innerText = "";
    let i = 0;
    const interval = setInterval(() => {
        if (i < logs.length) {
            logBox.innerText += logs[i] + "\n";
            i++;
        } else {
            clearInterval(interval);
        }
    }, 300);

    document.body.classList.add("screen-error");
    setTimeout(() => document.body.classList.remove("screen-error"), 1200);

    if (tentativas >= 3) {
        logBox.innerText += "\nSYSTEM WARNING:\nTOO MANY ATTEMPTS\nSESSION MONITORED...";
        alertaAcessoNegado();
    }
}

// ======================
// Caixa flutuante de ACESSO NEGADO
// ======================
function alertaAcessoNegado() {
    // Cria o container
    const box = document.createElement("div");
    box.id = "alerta-acesso-negado";
    box.style.position = "fixed";
    box.style.top = "50%";
    box.style.left = "50%";
    box.style.transform = "translate(-50%, -50%)"; // centraliza
    box.style.width = "400px";
    box.style.padding = "25px";
    box.style.backgroundColor = "rgba(200,0,0,0.95)";
    box.style.color = "#fff";
    box.style.fontFamily = "Consolas, monospace";
    box.style.textAlign = "center";
    box.style.fontSize = "1.5rem";
    box.style.border = "2px solid #fff";
    box.style.borderRadius = "10px";
    box.style.zIndex = "9999";
    box.style.boxShadow = "0 0 25px rgba(0,0,0,0.7)";
    box.style.animation = "pulseRedBox 1s infinite alternate";
    box.style.pointerEvents = "none";

    box.innerHTML = `
        <div style="font-size:1.4rem; font-weight:bold;">!!! ACESSO NEGADO !!!</div>
        <div style="margin-top:10px;">VOCÊ NÃO ESTÁ AUTORIZADO A ACESSAR ESTA ÁREA</div>
    `;

    document.body.appendChild(box);

    // Remove automaticamente após 5 segundos
    setTimeout(() => {
        if (box.parentNode) box.parentNode.removeChild(box);
    }, 5000);
}

// ======================
// Inicializa o terminal
// ======================
function initTerminal() {
    document.body.classList.add("terminal-mode");

    if (window.falhaLogin === true) {
        erroDOS();
        alertaAcessoNegado();
    }

    const voltarForm = document.getElementById("voltarForm");
    if (voltarForm) {
        voltarForm.addEventListener("submit", function () {
            sessionStorage.clear();
            localStorage.clear();

            // Limpa logs
            const logBox = document.getElementById("dos-log");
            if (logBox) logBox.innerText = "";

            // Remove alerta se estiver visível
            const box = document.getElementById("alerta-acesso-negado");
            if (box && box.parentNode) box.parentNode.removeChild(box);
        });
    }
}

// ======================
// Adiciona animação de pulso vermelho
// ======================
const style = document.createElement("style");
style.innerHTML = `
@keyframes pulseRedBox {
    from { background-color: rgba(200,0,0,0.85); }
    to { background-color: rgba(255,0,0,0.95); }
}
#alerta-acesso-negado {
    animation: pulseRedBox 1s infinite alternate;
}
`;
document.head.appendChild(style);

// ======================
// Executa após DOM carregado
// ======================
document.addEventListener("DOMContentLoaded", initTerminal);