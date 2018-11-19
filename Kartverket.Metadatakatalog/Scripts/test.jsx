class Test extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            active: false
        };
    }
    getMapItems() {
        if (localStorage.mapItems) {
            return JSON.parse(localStorage.mapItems);
        } else {
            return null;
        }
    }
    renderMapItemList() {
        var mapItems = this.getMapItems();
        if (mapItems) {
            let mapItemElements = this.getMapItems().map(function (mapItem, i) {
                return <li key={i}>{mapItem.name}</li>;
            });
            let mapListElement = React.createElement('ul', { className: 'mapItemList' }, mapItemElements);
            return mapListElement;
        }
    }
    render() {
        return (
            <div className={this.state.active ? 'map' : 'hidden'}>
                <div className="custom-modal">
                    <div className="custom-modal-container">
                        { this.renderMapItemList() }
                        <iframe style={{ height: '100%' }} />
                    </div>
                </div>
            </div>
        );
    }
}

ReactDOM.render(<Test />, document.getElementById('react-content'));