/*jslint browser: true, devel: true, node: true, sloppy: true */
/*global jQuery, CJS*/

"use strict";

(function ($) {
    
    var defaultOptions = {
            onChange : function (chemin) {},
            onValide : function (chemin) {},
            onAnnule : function () {}
        },
        LocalBrowser,
        getParent = function (chemin) {
            var arbo;
            
            if (chemin.slice(-1) === "\\") {
                chemin = chemin.slice(0, -1);
            }
            arbo = chemin.split("\\");
            arbo.pop();
            return arbo.join("\\");
        },
        cheminToLiens = function (chemin) {
            var arbo, i, retour = [];
            
            if (chemin.slice(-1) === "\\") {
                chemin = chemin.slice(0, -1);
            }
            arbo = chemin.split("\\");
            
            retour.push('<a href="#" class="dossier" data-dossier=""><i class="fa fa-desktop fa-lg" aria-hidden="true"></i> Ordinateur</a>');
            
            for (i = 0; i < arbo.length - 1; i += 1) {
                retour.push('<a href="#" class="dossier" data-dossier="' + arbo.slice(0, i + 1).join("\\") + '"><i class="fa fa-folder-o" aria-hidden="true"></i> ' + arbo[i] + '</a>');
            }
                        
            if (arbo[i] !== "") { retour.push(arbo[i]); }
                    
            return retour.join(" &gt; ");
        };
    
    LocalBrowser = function (handler, options) {
        $(handler).addClass("local-browser");
        this.handler        = handler;
        this.dossierChoisi  = "";
        $.extend(true, this, defaultOptions, options);
    };
    
    LocalBrowser.prototype.init = function () {
        var me = this;
        $(this.handler)
            .append('<span class="parent"><a href="#" class="dossier" data-dossier=""><i class="fa fa-desktop fa-lg" aria-hidden="true"></i> Ordinateur</a></span>')
            .append('<div class="liste-enfants">liste</div>')
            .append('<a href="#" class="bouton bout-annuler">Annuler</a> ')
            .append('<a href="#" class="bouton bout-nouveau-dossier inactif">Nouveau dossier</a> ')
            .append('<a href="#" class="bouton bout-confirme-choix">Choisir ce dossier</a><span class="dossier-choisi"></span>');
        
        $(this.handler).find(".bout-confirme-choix").click(function (e) {
            e.preventDefault();
            me.onValide(me.dossierChoisi);
        });
        
        $(this.handler).find(".bout-annuler").click(function (e) {
            e.preventDefault();
            me.onAnnule();
        });
        
        $(this.handler).find(".bout-nouveau-dossier").click(function (e) {
            e.preventDefault();
            console.log("ci");
            $(this).addClass("inactif");
            $(me.handler).find(".parent")
                .append('<span class="nouveau-dossier"> &gt; <input type="text" class="in-nouveau-dossier"> ' +
                        '<a href="" class="valide-nouveau-dossier inactif"><i class="fa fa-check fa-lg" aria-hidden="true"></i></a> ' +
                        '<a href="" class="annule-nouveau-dossier"><i class="fa fa-times fa-lg danger" aria-hidden="true"></i></a></span>');
            
            $(me.handler).find(".parent .in-nouveau-dossier").focus().keyup(function (e) {
                $(this).val($(this).val().replace(/[\/\\:\*\?"<>|]/g, ""));
                
                $(me.handler).find(".parent .valide-nouveau-dossier").toggleClass("inactif", $(me.handler).find(".parent .in-nouveau-dossier").val().trim() === "");
                if (e.originalEvent.keyCode === 13) {
                    me.nouveauDossier();
                } else if (e.originalEvent.keyCode === 27) {
                    $(me.handler).find(".parent .nouveau-dossier").remove();
                    $(me.handler).find(".bout-nouveau-dossier").removeClass("inactif");
                }
            });
            
            $(me.handler).find(".parent a.valide-nouveau-dossier").click(function (e) {
                e.preventDefault();
                me.nouveauDossier();
            });
            
            $(me.handler).find(".parent a.annule-nouveau-dossier").click(function (e) {
                e.preventDefault();
                $(me.handler).find(".parent .nouveau-dossier").remove();
                $(me.handler).find(".bout-nouveau-dossier").removeClass("inactif");
            });
            
            
        });
        
        this.getEnfants("");
    };
    
    LocalBrowser.prototype.nouveauDossier = function () {
        var params = {},
            me = this,
            dossierACreer = this.dossierChoisi + "\\" + $(this.handler).find(".parent .in-nouveau-dossier").val().trim();
        params[CJS.PARAM__DOSSIER] = dossierACreer;
        $(me.handler).find(".parent .nouveau-dossier").remove();
        $.post(
            "http://localhost:8000/" + CJS.ACTION__CREATION_DOSSIER,
            params,
            function () {
                me.dossierChoisi = dossierACreer;
                me.getEnfants(dossierACreer);
                $(me.handler).find(".parent").html(cheminToLiens(dossierACreer));
            }
        );
        
        console.log(params);
    };
                                                            
    LocalBrowser.prototype.getEnfants = function (parent) {
        var params  = {},
            me      = this;
                
        params[CJS.PARAM__DOSSIER] = parent;
        params[CJS.PARAM__DOSSIERS_SEUL] = true;

        $.post(
            "http://localhost:8000/" + CJS.ACTION__LISTE_DOSSIERS,
            params,
            function (dossiers) {
                var i;
                $(me.handler).find(".liste-enfants").html("");
                $(me.handler).find(".bout-nouveau-dossier").toggleClass("inactif", parent === "");
                
                if (parent !== "") {
                    $(me.handler).find(".liste-enfants").append('<a href="#" class="dossier" data-dossier="' + getParent(parent) + '"><i class="fa fa-folder-o fa-lg" aria-hidden="true"></i> ..</a><br>');
                }
                for (i = 0; i < dossiers.length; i += 1) {
                    $(me.handler).find(".liste-enfants").append('<a href="#" class="dossier in-liste" data-dossier="' + dossiers[i].CheminComplet + '"><i class="fa fa-folder-o fa-lg" aria-hidden="true"></i> ' + dossiers[i].Nom + '</a>');
                }

                $(".dossier").click(function (e) {
                    e.preventDefault();
                    me.dossierChoisi = $(this).attr("data-dossier");
                    me.onChange(me.dossierChoisi);
                    $(me.handler).find(".parent").html(cheminToLiens($(this).attr("data-dossier")));
                    me.getEnfants($(this).attr("data-dossier"));
                });
            }
        );
    };
    
    $.fn.LocalBrowser = function (options) {
        var i, retour = this;
        
        options = typeof options !== "undefined" ? options : {};
        
        this.each(function () {
            if (!this.localBrowser) {
                this.localBrowser = new LocalBrowser(this, options || {});
            }
            if (typeof options.action !== "undefined") {
                switch (options.action) {
                case "getDossierEnCours":
                    retour = this.localBrowser.dossierChoisi;
                    break;
                }
                console.log(options.action);
            } else {
                this.localBrowser.init();
            }
        });
        
        return retour;
    };
    
    $.fn.ChoixDossier = function (options) {
        return this.each(function () {
            var me = this,
                uniqueId = "id" + Math.floor(10000 * Math.random());
            
            while ($("#" + uniqueId).length > 0) { uniqueId = "id" + Math.floor(10000 * Math.random()); }

            options = options || {};
            
            
            if ($("#_hoverBg").length === 0) {
                $("body").append('<div id="_hoverBg"></div>');
            }
            if ($("#" + uniqueId).length === 0) {
                $("#_hoverBg").append('<div class="contenant-browser" id="' + uniqueId + '"><h2>Choisir un dossier local</h2><div class="browser"></div></div>');
                $("#" + uniqueId).LocalBrowser({
                    onValide : function (chemin) {
                        $(me).find("span.chemin-choisi").html(chemin || "(aucun dossier choisi)");
                        $(me).attr("data-chemin", chemin);
                        $("#_hoverBg").hide();
                        $("#" + uniqueId).hide();
                        if (typeof options.onChange !== "undefined") {
                            options.onChange.call(me, chemin);
                        }
                    },
                    onAnnule : function () {
                        $("#_hoverBg").hide();
                        $("#" + uniqueId).hide();
                    }
                });
                $("#" + uniqueId).hide();
            }
            
            $(this).addClass("bout-choix-dossier");
            $(this).attr("data-chemin", "");
            $(this).html('<span class="chemin-choisi">(aucun dossier choisi)</span><span class="bouton">Parcourir...</span>');
            
            $(this).click(function (e) {
                e.preventDefault();
                $("#_hoverBg").show();
                $("#" + uniqueId).show();
            });
        });
    };
    
}(jQuery));