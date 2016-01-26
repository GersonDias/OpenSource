/// <reference path='../_all.ts' />
var todos;
(function (todos_1) {
    'use strict';
    var todoStorage = (function () {
        function todoStorage() {
            this.STORAGE_ID = 'todos-angularjs-typescript';
        }
        todoStorage.prototype.workItemStorage = function (workItemId) {
            return this.STORAGE_ID + workItemId;
        };
        todoStorage.prototype.get = function (workItemId) {
            return JSON.parse(localStorage.getItem(this.workItemStorage(workItemId)) || '[]');
        };
        todoStorage.prototype.put = function (todos, workItemId) {
            localStorage.setItem(this.workItemStorage(workItemId), JSON.stringify(todos));
        };
        return todoStorage;
    })();
    todos_1.todoStorage = todoStorage;
})(todos || (todos = {}));
//# sourceMappingURL=TodoStorage.js.map