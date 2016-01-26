var todos;
(function (todos) {
    function disableEnterSubmit() {
        return {
            link: function ($scope, element, attributes) {
                element.bind('keydown', function (event) {
                    if (event.keyCode == 13) {
                        event.preventDefault();
                        return false;
                    }
                    ;
                });
            }
        };
    }
    todos.disableEnterSubmit = disableEnterSubmit;
})(todos || (todos = {}));
;
//# sourceMappingURL=disable-enter.js.map