/*jslint browser: true, devel: true, node: true, sloppy: true */
/*global $, CJS */
var Serveur = function (prefixe) {
    if (prefixe.slice(-1) !== "/") {
        prefixe += "/";
    }
    this.prefixe = prefixe;
};

Serveur.prototype.stop = function () {
    $.get(this.prefixe + "__stop");
};

Serveur.prototype.post = function (action, p1, p2) {
    if (typeof p2 === "undefined") {
        $.post(this.prefixe + action, {}, p1);
    } else {
        $.post(this.prefixe + action, p1, p2);
    }
    
};