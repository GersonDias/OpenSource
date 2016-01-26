/// <reference path='../_all.ts' />
/// <reference path="../../typings/tsd.d.ts" />
var todos;
(function (todos_1) {
    'use strict';
    var todoCtrl = (function () {
        function todoCtrl($scope, $location, todoStorage, filterFilter) {
            this.$scope = $scope;
            this.$location = $location;
            this.todoStorage = todoStorage;
            this.filterFilter = filterFilter;
            var self = this;
            VSS.require(['TFS/WorkItemTracking/Services'], function (workItemService) {
                workItemService.WorkItemFormService.getService().then(function (workItem) {
                    workItem.getId().then(function (workItemId) {
                        var todos = todoStorage.get(workItemId);
                        self.todos = todos;
                        self.workItemId = workItemId;
                        $scope.newTodo = '';
                        $scope.editedTodo = null;
                        $scope.vm = self;
                        $scope.todos = todos;
                        $scope.$apply();
                    });
                });
            });
            $scope.$watch('todos', function () { return self.onTodos(); }, true);
            $scope.$watch('location.path()', function (path) { return self.onPath(path); });
            if ($location.path() === '')
                $location.path('/');
            $scope.location = $location;
        }
        todoCtrl.prototype.onPath = function (path) {
            this.$scope.statusFilter = (path === '/active') ?
                { completed: false } : (path === '/completed') ?
                { completed: true } : {};
        };
        todoCtrl.prototype.onTodos = function () {
            this.$scope.remainingCount = this.filterFilter(this.todos, { completed: false }).length;
            this.$scope.doneCount = this.todos.length - this.$scope.remainingCount;
            this.$scope.allChecked = !this.$scope.remainingCount;
            this.todoStorage.put(this.todos, this.workItemId);
        };
        todoCtrl.prototype.addTodo = function () {
            var newTodo = this.$scope.newTodo.trim();
            if (!newTodo.length) {
                return;
            }
            ;
            this.todos.push(new todos_1.todoItem(newTodo, false, this.workItemId));
            this.$scope.newTodo = '';
        };
        todoCtrl.prototype.editTodo = function (todoItem) {
            this.$scope.editedTodo = todoItem;
            this.$scope.originalTodo = angular.extend({}, todoItem);
        };
        todoCtrl.prototype.revertEdits = function (todoItem) {
            this.todos[this.todos.indexOf(todoItem)] = this.$scope.originalTodo;
            this.$scope.reverted = true;
        };
        todoCtrl.prototype.doneEditing = function (todoItem) {
            this.$scope.editedTodo = null;
            this.$scope.originalTodo = null;
            if (this.$scope.reverted) {
                this.$scope.reverted = null;
                return;
            }
            todoItem.title = todoItem.title.trim();
            if (!todoItem.title)
                this.removeTodo(todoItem);
        };
        todoCtrl.prototype.removeTodo = function (todoItem) {
            this.todos.splice(this.todos.indexOf(todoItem), 1);
        };
        todoCtrl.prototype.clearDoneTodos = function () {
            this.$scope.todos = this.todos = this.todos.filter(function (todoItem) { return !todoItem.completed; });
        };
        todoCtrl.prototype.markAll = function (completed) {
            this.todos.forEach(function (todoItem) {
                todoItem.completed = completed;
            });
        };
        todoCtrl.$inject = [
            '$scope',
            '$location',
            'todoStorage',
            'filterFilter'
        ];
        return todoCtrl;
    })();
    todos_1.todoCtrl = todoCtrl;
})(todos || (todos = {}));
//# sourceMappingURL=todoCtrl.js.map