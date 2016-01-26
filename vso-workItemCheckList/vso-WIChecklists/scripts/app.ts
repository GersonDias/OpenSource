/// <reference path='../node_modules/vss-sdk/typings/VSS.d.ts' />

module todos {
    'use strict';

    var todomvc = angular.module('todomvc', []);
    todomvc.controller('todoCtrl', todoCtrl);
    todomvc.directive('todoBlur', todoBlur);
    todomvc.directive('todoFocus', todoFocus);
    todomvc.directive('todoEscape', todoEscape);
    todomvc.service('todoStorage', todoStorage);
}