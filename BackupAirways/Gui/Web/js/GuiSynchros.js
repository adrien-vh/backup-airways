/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $ */

var GuiSynchros = {};

// Gestion de l'interface utilisateurs
GuiSynchros.montreSousContenu = function (e) {
    e.preventDefault();
    var hauteur = $(this).parents(".sauvegarde").find(".contenant").height(),
        ref = $(this).attr("data-ref");

    $(this).parents(".sauvegarde").find(".sous-contenu." + ref).detach().insertAfter($(this).parents(".sauvegarde").find(".sous-contenu.existant"));

    $(this).parents(".sauvegarde").find(".cont-bouton.existant").hide();
    $(this).parents(".sauvegarde").find(".cont-bouton." + ref).show();

    $(this).parents(".sauvegarde").find(".contenu").animate({ marginTop: "-" + hauteur + "px" }, 300);
};

GuiSynchros.cacheSousContenu = function (e) {
    e.preventDefault();
    var hauteur = $(this).parents(".sauvegarde").find(".contenant").height(),
        ref = $(this).attr("data-ref");

    $(this).parents(".sauvegarde").find(".cont-bouton").hide();
    $(this).parents(".sauvegarde").find(".cont-bouton.existant").show();

    $(this).parents(".sauvegarde").find(".contenu").animate({ marginTop: "0px" }, 300);
};