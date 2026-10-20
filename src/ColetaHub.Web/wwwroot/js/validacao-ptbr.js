// Ajustes do jQuery Validate para pt-BR e adaptador do atributo [DataNaoFutura].
(function ($) {
    'use strict';

    // "1.234,56" → 1234.56. Retorna NaN para formatos inválidos (ex.: "2.5", que no Brasil é ambíguo).
    function numeroBrasileiro(valor) {
        var texto = $.trim(valor);
        if (!/^-?(\d{1,3}(\.\d{3})+|\d+)(,\d+)?$/.test(texto)) {
            return NaN;
        }
        return parseFloat(texto.replace(/\./g, '').replace(',', '.'));
    }

    // Mensagens padrão em português, usadas quando a regra vem de um atributo HTML e não de uma Data Annotation.
    $.extend($.validator.messages, {
        required: 'Campo obrigatório.',
        number: 'Informe um número válido, com vírgula nas casas decimais.',
        date: 'Informe uma data válida.',
        maxlength: $.validator.format('Use no máximo {0} caracteres.'),
        minlength: $.validator.format('Use pelo menos {0} caracteres.'),
        range: $.validator.format('Informe um valor entre {0} e {1}.')
    });

    // Por padrão o jQuery Validate só entende ponto decimal; o servidor (cultura pt-BR) espera vírgula.
    $.validator.methods.number = function (valor, elemento) {
        return this.optional(elemento) || !isNaN(numeroBrasileiro(valor));
    };

    $.validator.methods.range = function (valor, elemento, parametros) {
        var numero = numeroBrasileiro(valor);
        return this.optional(elemento) || (numero >= Number(parametros[0]) && numero <= Number(parametros[1]));
    };

    // [DataNaoFutura] emite data-val-datanaofutura no input; os valores de <input type="date"> são yyyy-MM-dd.
    function hojeIso() {
        var hoje = new Date();
        var mes = String(hoje.getMonth() + 1).padStart(2, '0');
        var dia = String(hoje.getDate()).padStart(2, '0');
        return hoje.getFullYear() + '-' + mes + '-' + dia;
    }

    $.validator.addMethod('datanaofutura', function (valor, elemento) {
        return this.optional(elemento) || valor <= hojeIso();
    });

    $.validator.unobtrusive.adapters.addBool('datanaofutura');
})(jQuery);
