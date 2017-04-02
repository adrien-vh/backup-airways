/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS,  Serveur, rivets */

"use strict";

var LocalBrowserCtrl = function (destChemin, caller, onChange) {
    var me = this,
        uniqueId = "id" + Math.floor(10000 * Math.random());

    while ($("#" + uniqueId).length > 0) { uniqueId = "id" + Math.floor(10000 * Math.random()); }

    me.uniqueId = uniqueId;
    me.cheminChoisi = "";
    me.destChemin = destChemin;
    me.caller = caller;
    me.onChange = onChange;


    if ($("#_hoverBg").length === 0) { $("body").append('<div id="_hoverBg"></div>'); }

    if ($("#" + me.uniqueId).length === 0) {
        $("#_hoverBg").append('<div class="contenant-browser" id="' + me.uniqueId + '"><h2>Choisir un dossier local</h2><div class="browser"></div></div>');
        $("#" + me.uniqueId).LocalBrowser({
            onValide: function (chemin) {
                me.cheminChoisi = chemin;
                me.destChemin.val = chemin;
                $("#_hoverBg").hide();
                $("#" + me.uniqueId).hide();

                if (typeof me.caller !== "undefined" && typeof me.onChange !== "undefined") {
                    me.onChange.apply(caller);
                }
            },
            onAnnule: function () {
                $("#_hoverBg").hide();
                $("#" + me.uniqueId).hide();
            }
        });
        $("#" + me.uniqueId).hide();
    }
};

LocalBrowserCtrl.prototype.showBrowser = function (e, localBrowserCtrl) {
    e.preventDefault();
    $("#_hoverBg").show();
    $("#" + localBrowserCtrl.uniqueId).show();
};

// Composant « Browser Local »
rivets.components['local-browser'] = {
    template: function () {
        return '<a href="#" rv-on-click="showBrowser" class="bout-choix-dossier">' +
            '<span rv-show="cheminChoisi | nonVide">{ cheminChoisi }</span>' +
            '<span rv-show="cheminChoisi | vide">(aucun dossier choisi)</span> ' +
            '<span class="bouton">Parcourir...</span>' +
            '</a>';
    },

    initialize: function (el, data) {
        console.log(data);
        return new LocalBrowserCtrl(data.chemin, data.caller, data.onChange);
    }
};
