// Comportamentos de interface do ColetaHub (sem dependência além do Bootstrap).
(function () {
    'use strict';

    // Alertas de TempData somem sozinhos após 5 segundos.
    document.querySelectorAll('.alerta--auto').forEach(function (alerta) {
        setTimeout(function () {
            bootstrap.Alert.getOrCreateInstance(alerta).close();
        }, 5000);
    });

    // Modal de exclusão: recebe a URL e o código do botão <botao-acao tipo="excluir" modal="true"> que o abriu.
    var modal = document.getElementById('modalExclusao');
    if (modal) {
        modal.addEventListener('show.bs.modal', function (evento) {
            var botao = evento.relatedTarget;
            if (!botao) { return; }
            document.getElementById('formExclusao').action = botao.getAttribute('href');
            modal.querySelector('[data-modal-descricao]').textContent = botao.dataset.descricao;
        });
    }

    // Código de rastreio sempre em maiúsculas.
    document.querySelectorAll('[data-maiusculas]').forEach(function (campo) {
        campo.addEventListener('input', function () {
            var posicao = campo.selectionStart;
            campo.value = campo.value.toUpperCase().replace(/\s/g, '');
            campo.setSelectionRange(posicao, posicao);
        });
    });

    // Máscara de CPF: 000.000.000-00.
    document.querySelectorAll('[data-mascara-cpf]').forEach(function (campo) {
        campo.addEventListener('input', function () {
            var d = campo.value.replace(/\D/g, '').slice(0, 11);
            campo.value = d
                .replace(/^(\d{3})(\d)/, '$1.$2')
                .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
                .replace(/\.(\d{3})(\d{1,2})$/, '.$1-$2');
        });
    });

    // Em um cadastro novo, peso e valor começam vazios em vez de "0".
    var idPacote = document.getElementById('Pacote_Id');
    if (idPacote && idPacote.value === '0') {
        document.querySelectorAll('[data-limpar-zero]').forEach(function (campo) {
            if (/^0([.,]0+)?$/.test(campo.value)) { campo.value = ''; }
        });
    }

    // Data de saída só é editável quando o status indica que o pacote saiu (Retirado ou Coletado).
    var status = document.querySelector('[data-status-com-saida]');
    var dataSaida = document.querySelector('[data-data-saida]');
    if (status && dataSaida) {
        var statusComSaida = status.dataset.statusComSaida.split(',');
        var atualizarDataSaida = function () {
            var permite = statusComSaida.indexOf(status.value) >= 0;
            dataSaida.disabled = !permite;
            if (!permite) { dataSaida.value = ''; }
        };
        status.addEventListener('change', atualizarDataSaida);
        atualizarDataSaida();
    }

    // Contador de caracteres das observações.
    document.querySelectorAll('[data-contador]').forEach(function (campo) {
        var alvo = document.getElementById(campo.dataset.contador);
        var limite = campo.getAttribute('maxlength');
        var atualizar = function () { alvo.textContent = campo.value.length + '/' + limite; };
        campo.addEventListener('input', atualizar);
        atualizar();
    });
})();
