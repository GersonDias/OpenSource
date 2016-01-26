/// <reference path='../_all.ts' />
var todos;
(function (todos) {
    'use strict';
    var todoItem = (function () {
        function todoItem(title, completed, workItemId) {
            this.title = title;
            this.completed = completed;
            this.workItemId = workItemId;
        }
        return todoItem;
    })();
    todos.todoItem = todoItem;
})(todos || (todos = {}));
//# sourceMappingURL=TodoItem.js.map