﻿$(function () {
    $.validator.addMethod(
        'c_attachments_required',
        function (value, element) {
            var $control = $('[id="' + $(element).data('name') + '.items"]');
            return $control.find('.control-attachments-item:not(.preparation-delete)').length > 0;
        }
    );
    $.validator.addMethod(
        'c_num',
        function (value, element) {
            return this.optional(element) || /^(-)?(¥|\\|\$)?[\d,.]+$/.test(value);
        }
    );
    $.validator.addMethod(
        'c_min_num',
        function (value, element, params) {
            return this.optional(element) ||
                parseFloat(value.replace(/[\uC2A5|\u005C,¥]/g, '')) >= parseFloat(params);
        }
    );
    $.validator.addMethod(
        'c_max_num',
        function (value, element, params) {
            return this.optional(element) ||
                parseFloat(value.replace(/[\uC2A5|\u005C,¥]/g, '')) <= parseFloat(params);
        }
    );
    $.validator.addMethod(
        'c_regex',
        function (value, element, params) {
            try{
                return this.optional(element) || new RegExp(params).test(value);
            }
            catch(e){
                return false;
            }
        }
    );
    $.validator.addMethod(
        'maxlength',
        function (value, element, params) {
            try {
                if ($('#data-validation-maxlength-type').val() === 'Regex') {
                    return (value.length
                        + value.replace(
                            new RegExp('['
                                + $('#data-validation-maxlength-regex').val()
                                + ']', 'g'), '').length) <= parseFloat(params);
                } else {
                    return value.length <= parseFloat(params);
                }
            }
            catch(e){
                return false;
            }
        }
    );

    $p.setValidationError = function ($form) {
        $form.find('.ui-tabs li').each(function () {
            $('.control-markdown.error').each(function () {
                $p.toggleEditor($(this), true);
            });
            var $control = $('#' + $(this)
                .attr('aria-controls'))
                .find('input.error:first:not(.search)');
            if ($control.length === 1) {
                $(this).closest('.ui-tabs').tabs('option', 'active', $(this).index());
                $control.focus();
            }
        });
    }

    $p.formValidate = function ($form, $control) {
        $('input, select, textarea').each(function () {
            $(this).rules('remove');
        });
        if (!$control.data('validations') || $control.hasClass('merge-validations')) {
            $p.applyValidator();
        }
        if ($control.data('validations')) {
            $.each($control.data('validations'), function (i, validation) {
                var $target = $p.getControl(validation.ColumnName);
                if (validation.Required) {
                    if ($target.hasClass('control-attachments')) {
                        $target.rules('add', {
                            c_attachments_required: true
                        });
                    } else {
                        $target.rules('add', {
                            required: true
                        });
                    }
                }
                if (validation.ClientRegexValidation) {
                    $target.rules('add', {
                        c_regex: validation.ClientRegexValidation,
                        messages: { c_regex: validation.RegexValidationMessage }
                    });
                }
                if (validation.Min != undefined) {
                    $target.rules('add', {
                        c_min_num: validation.Min
                    });
                }
                if (validation.Max != undefined) {
                    $target.rules('add', {
                        c_max_num: validation.Max
                    });
                }
            });
        }
        $form.validate({
            errorPlacement: function(error, element) {
                if(!element.closest('.field-markdown').length){
                    element.closest('.container-normal').append(error);
                }else{
                    error.insertAfter(element);
                }
            }
        });
    }
    $p.applyValidator();
});
$p.applyValidator = function () {
    $.extend($.validator.messages, {
        required: $p.display('ValidateRequired'),
        c_attachments_required: $p.display('ValidateRequired'),
        c_num: $p.display('ValidateNumber'),
        c_min_num: $p.display('ValidateMinNumber'),
        c_max_num: $p.display('ValidateMaxNumber'),
        date: $p.display('ValidateDate'),
        email: $p.display('ValidateEmail'),
        equalTo: $p.display('ValidateEqualTo'),
        maxlength: $p.display('ValidateMaxLength'),
        url: $p.display('ValidationFormat'),
        number: $p.display('ValidationFormat'),
        digits: $p.display('ValidationFormat'),
        creditcard: $p.display('ValidationFormat'),
        minlength: $p.display('ValidationFormat'),
        rangelength: $p.display('ValidationFormat'),
        range: $p.display('ValidationFormat'),
        max: $p.display('ValidationFormat'),
        min: $p.display('ValidationFormat')
    });
    $('form').each(function () {
        $(this).validate({
            ignore: '',
            errorPlacement: function(error, element) {
                if(!element.closest('.field-markdown').length){
                    element.closest('.container-normal').append(error);
                }else{
                    error.insertAfter(element);
                }
            }
        });
    });
    $('[data-validate-required="1"]').each(function () {
        $(this).rules('add', { required: true });
    });
    $('[data-validate-attachments-required="1"]').each(function () {
        $(this).rules('add', { c_attachments_required: true });
    });
    $('[data-validate-number="1"]').each(function () {
        $(this).rules('add', { c_num: true });
    });
    $('[data-validate-min-number]').each(function () {
        $(this).rules('add', { c_min_num: $(this).attr('data-validate-min-number') });
    });
    $('[data-validate-max-number]').each(function () {
        $(this).rules('add', { c_max_num: $(this).attr('data-validate-max-number') });
    });
    $('[data-validate-date="1"]').each(function () {
        $(this).rules('add', { date: true });
    });
    $('[data-validate-email="1"]').each(function () {
        $(this).rules('add', { email: true });
    });
    $('[data-validate-equal-to]').each(function () {
        $(this).rules('add', { equalTo: $(this).attr('data-validate-equal-to') });
    });
    $('[data-validate-maxlength]').each(function () {
        $(this).rules('add', {
            maxlength: $(this).attr('data-validate-maxlength'),
            messages: { maxlength: $p.display('ValidateMaxLength').replace('{0}', $(this).attr('data-validate-maxlength')) }
        });
    });
    $('[data-validate-regex]').each(function () {
        $(this).rules('add', {
            c_regex: $(this).attr('data-validate-regex'),
            messages: { c_regex: $(this).attr('data-validate-regex-errormessage') }
        });
    });
}