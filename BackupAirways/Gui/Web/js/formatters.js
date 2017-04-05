/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, rivets */

"use strict";

rivets.formatters.eq = function (value1, value2) { return value1 === value2; };
rivets.formatters.neq = function (value1, value2) { return value1 !== value2; };
rivets.formatters.gt = function (value1, value2) { return value1 > value2; };
rivets.formatters.minus = function (value1, value2) { return value1 - value2; };
rivets.formatters.vide = function (value1) { return $.isArray(value1) ? value1.length === 0 : value1.trim() === ""; };
rivets.formatters.nonVide = function (value1) { return $.isArray(value1) ? value1.length !== 0 : value1.trim() !== ""; };
rivets.formatters.prefixe = function (value1, value2) { return value2 + value1; };
rivets.formatters.plusLongQue = function (value1, value2) { return value1.length > value2; };
rivets.formatters.lower = function (value1) { return value1.toLowerCase(); };
rivets.formatters.upper = function (value1) { return value1.toUpperCase(); };
rivets.formatters.nonVideVide = function (value1, value2) { return (($.isArray(value2) ? value2.length === 0 : value2.trim() === "") && ($.isArray(value1) ? value1.length !== 0 : value1.trim() !== "")); };