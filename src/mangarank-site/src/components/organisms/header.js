import React from 'react'
import { Link } from 'gatsby'
import {
  Container,
  Navbar,
  NavbarBrand,
  NavbarToggler,
  Collapse,
  Nav,
  NavItem,
  NavLink
} from 'reactstrap'

class Header extends React.Component {
  constructor(props) {
    super(props)
    this.toggle = this.toggle.bind(this)
    this.state = {
      isOpen: false
    }
  }

  toggle() {
    this.setState({
      isOpen: !this.state.isOpen
    })
  }

  render() {
    return (
      <Navbar color="dark" dark expand="lg">
        <Container>
          <NavbarBrand tag={Link} to="/">
            {this.props.siteTitle}
          </NavbarBrand>
          <NavbarToggler onClick={this.toggle}
                         aria-label="Toggle navigation"/>
          <Collapse isOpen={this.state.isOpen} navbar>
            <Nav navbar className="mr-auto">
              <NavItem>
                <NavLink tag={Link} to="/items">
                  マンガ
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={Link} to="/items/tagged/少年マンガ">
                  少年マンガ
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={Link} to="/items/tagged/青年マンガ">
                  青年マンガ
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={Link} to="/items/tagged/少女マンガ">
                  少女マンガ
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={Link} to="/items/tagged/女性マンガ">
                  女性マンガ
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={Link} to="/tags">
                  タグ
                </NavLink>
              </NavItem>
            </Nav>
            <Nav navbar>
              <NavItem>
                <NavLink tag={Link} to="/about">
                  サイトについて
                </NavLink>
              </NavItem>
            </Nav>
          </Collapse>
        </Container>
      </Navbar>
    )
  }
}

export default Header
