/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS, Mustache, Serveur */

"use strict";

$(function () {

    var params = {},
        cloudsAffiches = true,
        listeClients = [],
        serveur = new Serveur("http://localhost:8000/");
    
    function formatteSynchro(synchro) {
        var i;
                
        for (i = 0; i < synchro.Clients.length; i += 1) {
            synchro.Clients[i].deltaFichiers = synchro.NbFichiersMaitre - synchro.Clients[i].NbFichiers;
            synchro.Clients[i].complete = (synchro.Clients[i].deltaFichiers === 0);
        }
    }
    
    function recupSauvegardes() {
        serveur.post(
            CJS.ACTION__LISTE_SAUVEGARDES,
            function (sauvegardes) {
                var template = $('#tplSauvegarde').html(), rendered, i, j, synchros = sauvegardes.maitres.concat(sauvegardes.esclaves).concat(sauvegardes.inutilisees), synchro;

                
                
                //Mustache.parse(template);
                $(".sauvegarde.esclave").remove();
                $(".sauvegarde.maitre").remove();
                $(".sauvegarde.inutilisee").remove();

                template = $('#tplSynchro').html();
                
                for (i = 0; i < synchros.length; i += 1) {
                    synchro = synchros[i];
                    synchro.classe = synchro.Type === 1 ? "maitre " : (synchro.Type === 0 ? "esclave " : "inutilisee ");
                    synchro.classe += synchro.Valide ? "actif" : "inactif";
                    synchro.maitre = (synchro.Type === 1);
                    synchro.esclave = (synchro.Type === 0);
                    synchro.inutilisee = (synchro.Type === 2);
                    formatteSynchro(synchro);
                    rendered = Mustache.render(template, synchro);
                    $('#listeSauvegardesMaitres').append(rendered);
                }
                
                $(".sauvegarde.inutilisee .dossier-local").ChoixDossier({
                    onChange : function (chemin) {
                        $("a.bouton[data-synchro='" + $(this).attr("data-synchro") + "']").toggleClass("inactif", chemin === "");
                    }
                });
                
                $(".sauvegarde.inutilisee a.bouton").click(function (e) {
                    e.preventDefault();
                    var params = {};
                    params[CJS.PARAM__DOSSIER] = $("a.dossier-local[data-synchro='" + $(this).attr("data-synchro") + "']").attr("data-chemin");
                    params[CJS.PARAM__NOM_SAUVEGARDE] = $(this).attr("data-synchro");
                    
                    serveur.post(
                        CJS.ACTION__JOINDRE_SAUVEGARDE,
                        params,
                        function (retour) {
                            recupSauvegardes();
                        }
                    );
                    
                    console.log(params);
                });
            }
        );
    }
    
    
    // Validation du nom de la machine
    function valideNomMachine() {
        $("#inNomMachine").val($("#inNomMachine").val().trim());
        var nomInValide = /[^a-zA-Z0-9]/.test($("#inNomMachine").val()) || $("#inNomMachine").val().trim() === "",
            nomExistant = listeClients.indexOf($("#inNomMachine").val().toLowerCase()) !== -1;
        
        $("#boutConfirmeNomMachine").toggleClass("inactif", nomInValide || nomExistant);
        
        if (nomInValide) {
            $("#erreurNomClient").html("Le nom choisi n'est pas valide.");
            $("#erreurNomClient").show();
        } else if (nomExistant) {
            $("#erreurNomClient").html("Un client porte déjà ce nom.");
            $("#erreurNomClient").show();
        } else {
            $("#erreurNomClient").hide();
        }
    }
    
    // Récupération de la liste des clients
    function recupClients() {
        serveur.post(
            CJS.ACTION__GET_LISTE_CLIENTS,
            function (clients) {
                var i;
                for (i = 0; i < clients.length; i += 1) { clients[i] = clients[i].toLowerCase(); }
                listeClients = clients;
                valideNomMachine();
            }
        );
    }
    
    // Récupération de l'état de l'application
    serveur.post(
        CJS.ACTION__ETAT_INITIALISATION,
        function (etat) {
            $(".page").hide();
            if (etat[CJS.PARAM__EST_INITIALISE]) {
                $("#pageListeSauvegardes").show();
                recupSauvegardes();
            } else {
                $("#pageChoixTampon").show();
                /**/
               // recupClients();
            //    $("#pageChoixNom").show();
                /**/
            }
        }
    );
    

    
    /*****************************************************************************/
    /* -== PAGE CHOIX DU DOSSIER TAMPON ==-                                      */
    
    // Récupération des chemins vers les clouds installés sur le poste
    serveur.post(CJS.ACTION__CHEMINS_DRIVES, function (data) {
        if (typeof data[CJS.REP__CHEMIN_GDRIVE] !== "undefined") {
            $(".choix-cloud span.gdrive.inactif").hide();
            $(".choix-cloud span.gdrive.actif").css("display", "inline-block");
            $(".choix-cloud span.gdrive .chemin").html(data[CJS.REP__CHEMIN_GDRIVE]);
        }
    });

    // Mise en place du bouton parcourir
    $("#pageChoixTampon .dossier-local").ChoixDossier({ onChange : function (chemin) { $("#pageChoixTampon .autre-cloud .chemin").html(chemin); $("#boutConfirmeDossierTampon").toggleClass("inactif", chemin === ""); }});
    
    // Au changement de choix de cloud
    $(".choix-cloud span.actif").click(function (e) {
        $(".choix-cloud span.actif.choisi").removeClass("choisi");
        $(this).addClass("choisi");
        $("#boutConfirmeDossierTampon").toggleClass("inactif", $(".choix-cloud span.actif.choisi .chemin").html() === "");
    });
    
    // À la confirmation du dossier tampon
    $("#boutConfirmeDossierTampon").click(function (e) {
        e.preventDefault();
        
        var params  = {};
                
        params[CJS.PARAM__DOSSIER_TAMPON] = $(".choix-cloud span.actif.choisi .chemin").html();
        
        serveur.post(
            CJS.ACTION__SET_DOSSIER_TAMPON,
            params,
            function () {
                $(".page").hide();
                $("#pageChoixNom").show();
                //recupSauvegardes();
                recupClients();
            }
        );
    });

    /* -== FIN PAGE CHOIX DU DOSSIER TAMPON ==-                                  */
    /*****************************************************************************/
    
    /*****************************************************************************/
    /* -== PAGE CHOIX DU NOM DU CLIENT ==-                                       */

    
    // Récupération du nom de la machine
    serveur.post(
        CJS.ACTION__GET_NOM_MACHINE,
        function (retour) { $("#inNomMachine").val(retour[CJS.PARAM__NOM_MACHINE]); }
    );
    
    $("#inNomMachine").keyup(function (e) {
        e.preventDefault();
        valideNomMachine();
    });
    
    $("#boutConfirmeNomMachine").click(function (e) {
        e.preventDefault();
        var params = {};
        params[CJS.PARAM__NOM_MACHINE] = $("#inNomMachine").val();
        serveur.post(
            CJS.ACTION__CHANGE_NOM_CLIENT,
            params,
            function () {
                recupSauvegardes();
                $(".page").hide();
                $("#pageListeSauvegardes").show();
            }
        );
    });
    
    /* -== FIN PAGE CHOIX DU DOSSIER TAMPON ==-                                  */
    /*****************************************************************************/
    
    /*****************************************************************************/
    /* -== PAGE LISTE DES SAUVEGARDES ==-                                        */
        
    // Validation du nom de la nouvelle sauvegarde
    function valideNomSauvegarde() {
        $("#inNomSauvegarde").val($("#inNomSauvegarde").val().trim());
        var nomInValide = /[^a-zA-Z0-9 ]/.test($("#inNomSauvegarde").val()) || $("#inNomSauvegarde").val().trim() === "" || $("#inNomSauvegarde").val().length < 3,
            cheminInvalide = $("#dossierASauvegarder").attr("data-chemin") ? false : true;
        
        $("#boutNouvelleSauvegarde").toggleClass("inactif", nomInValide || cheminInvalide);
        
        if (nomInValide) {
            $("#erreurNouvelleSauvegarde span").html("Le nom choisi n'est pas valide (au moins 3 caractères sans caractères spéciaux).");
            $("#erreurNouvelleSauvegarde").show();
        } else if (cheminInvalide) {
            $("#erreurNouvelleSauvegarde span").html("Le dossier choisi n'est pas valide.");
            $("#erreurNouvelleSauvegarde").show();
        } else {
            $("#erreurNouvelleSauvegarde").hide();
        }
    }
    
    valideNomSauvegarde();
    
    $("#inNomSauvegarde").keyup(function (e) {
        e.preventDefault();
        valideNomSauvegarde();
    });
    
    $("#dossierASauvegarder").ChoixDossier({ onChange : function () { valideNomSauvegarde(); } });
    
    $("#boutNouvelleSauvegarde").click(function (e) {
        e.preventDefault();
        var params = {};
        params[CJS.PARAM__DOSSIER] = $("#dossierASauvegarder").attr("data-chemin");
        params[CJS.PARAM__NOM_SAUVEGARDE] = $("#inNomSauvegarde").val();
        serveur.post(
            CJS.ACTION__NOUVELLE_SAUVEGARDE,
            params,
            function () {
                recupSauvegardes();
            }
        );
    });
    
    /* -== FIN PAGE LISTE DES SAUVEGARDES ==-                                    */
    /*****************************************************************************/
    

});